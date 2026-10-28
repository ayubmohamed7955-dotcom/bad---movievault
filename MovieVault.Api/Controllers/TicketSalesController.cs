using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Data;
using MovieVault.Api.DTOs;
using MovieVault.Api.Entities;

namespace MovieVault.Api.Controllers;

[ApiController]
[Route("api/ticket-sales")]
[Produces("application/json")]
public class TicketSalesController : ControllerBase
{
    private readonly MovieVaultDbContext _db;

    public TicketSalesController(MovieVaultDbContext db)
    {
        _db = db;
    }

    /// <summary>Henter alle billetsalg. Kan filtreres på screening med ?screeningId=3.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketSaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketSaleDto>>> GetTicketSales([FromQuery] int? screeningId)
    {
        var query = _db.TicketSales.AsQueryable();
        if (screeningId is not null)
            query = query.Where(t => t.ScreeningId == screeningId);

        var sales = await query
            .OrderByDescending(t => t.SaleDate)
            .Select(ToDtoProjection)
            .ToListAsync();

        return Ok(sales);
    }

    /// <summary>Henter ét billetsalg.</summary>
    [HttpGet("{id:int}", Name = nameof(GetTicketSale))]
    [ProducesResponseType(typeof(TicketSaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketSaleDto>> GetTicketSale(int id)
    {
        var sale = await _db.TicketSales
            .Where(t => t.TicketSaleId == id)
            .Select(ToDtoProjection)
            .FirstOrDefaultAsync();

        if (sale is null)
            return Problem(statusCode: 404, title: "Ticket sale not found", detail: $"No ticket sale with id {id}.");

        return Ok(sale);
    }

    /// <summary>
    /// Registrerer et billetsalg. Tjekker at screeningen findes, ikke er annulleret eller startet,
    /// og at salget ikke får screeningen til at overstige salens kapacitet.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketSaleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketSaleDto>> CreateTicketSale(CreateTicketSaleDto dto)
    {
        // Serializable transaktion: to samtidige salg kan ikke begge se "der er plads" og overbooke salen
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var screening = await _db.Screenings
            .Include(s => s.Screen)
            .FirstOrDefaultAsync(s => s.ScreeningId == dto.ScreeningId);

        if (screening is null)
        {
            ModelState.AddModelError(nameof(dto.ScreeningId), $"No screening with id {dto.ScreeningId}.");
            return ValidationProblem(ModelState);
        }

        if (screening.Status == ScreeningStatus.Cancelled)
            ModelState.AddModelError(nameof(dto.ScreeningId), "Tickets cannot be sold for a cancelled screening.");

        if (screening.StartTime <= DateTime.Now)
            ModelState.AddModelError(nameof(dto.ScreeningId), "Tickets cannot be sold for a screening that has already started.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var alreadySold = await TicketsSoldFor(screening.ScreeningId);
        var available = screening.Screen.SeatingCapacity - alreadySold;
        if (dto.TotalTickets > available)
        {
            ModelState.AddModelError(nameof(dto.TotalTickets),
                $"Not enough seats. Capacity is {screening.Screen.SeatingCapacity}, {alreadySold} already sold, {available} available.");
            return ValidationProblem(ModelState);
        }

        var sale = new TicketSale
        {
            ScreeningId = dto.ScreeningId,
            TotalTickets = dto.TotalTickets,
            SaleDate = DateTime.Now // sættes af serveren, ikke af klienten
        };

        _db.TicketSales.Add(sale);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        var created = await _db.TicketSales
            .Where(t => t.TicketSaleId == sale.TicketSaleId)
            .Select(ToDtoProjection)
            .FirstAsync();

        return CreatedAtRoute(nameof(GetTicketSale), new { id = sale.TicketSaleId }, created);
    }

    /// <summary>
    /// Ændrer antallet af billetter i et salg. Kapaciteten tjekkes uden salgets eget gamle antal.
    /// Et salg kan ikke flyttes til en anden screening - det er i så fald et nyt salg.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TicketSaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketSaleDto>> UpdateTicketSale(int id, UpdateTicketSaleDto dto)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var sale = await _db.TicketSales
            .Include(t => t.Screening).ThenInclude(s => s.Screen)
            .FirstOrDefaultAsync(t => t.TicketSaleId == id);

        if (sale is null)
            return Problem(statusCode: 404, title: "Ticket sale not found", detail: $"No ticket sale with id {id}.");

        var isIncrease = dto.TotalTickets > sale.TotalTickets;

        // Færre billetter (fx refundering) er altid tilladt. Flere billetter kræver en aktiv, fremtidig screening.
        if (isIncrease && sale.Screening.Status == ScreeningStatus.Cancelled)
            ModelState.AddModelError(nameof(dto.TotalTickets), "Cannot add tickets to a sale for a cancelled screening.");

        if (isIncrease && sale.Screening.StartTime <= DateTime.Now)
            ModelState.AddModelError(nameof(dto.TotalTickets), "Cannot add tickets after the screening has started.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // Træk salgets eget gamle antal fra, ellers tælles det dobbelt
        var soldByOthers = await TicketsSoldFor(sale.ScreeningId) - sale.TotalTickets;
        var capacity = sale.Screening.Screen.SeatingCapacity;
        if (soldByOthers + dto.TotalTickets > capacity)
        {
            ModelState.AddModelError(nameof(dto.TotalTickets),
                $"Not enough seats. Capacity is {capacity}, {soldByOthers} sold in other sales, {capacity - soldByOthers} available for this sale.");
            return ValidationProblem(ModelState);
        }

        sale.TotalTickets = dto.TotalTickets;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        var updated = await _db.TicketSales
            .Where(t => t.TicketSaleId == id)
            .Select(ToDtoProjection)
            .FirstAsync();

        return Ok(updated);
    }

    /// <summary>Sletter et billetsalg (fx ved fuld refundering).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTicketSale(int id)
    {
        var sale = await _db.TicketSales.FindAsync(id);
        if (sale is null)
            return Problem(statusCode: 404, title: "Ticket sale not found", detail: $"No ticket sale with id {id}.");

        _db.TicketSales.Remove(sale);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Query 4: Billetsalg per film - antal solgte billetter og samlet omsætning.
    /// Salg til annullerede screenings tælles ikke med. Film uden salg vises med 0.
    /// </summary>
    [HttpGet("by-movie")]
    [ProducesResponseType(typeof(IEnumerable<MovieTicketSalesDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MovieTicketSalesDto>>> GetTicketSalesByMovie()
    {
        // Movie -> Screenings -> TicketSales. SUM over billetter og over (antal * pris) pr. film.
        var result = await _db.Movies
            .Select(m => new MovieTicketSalesDto
            {
                MovieId = m.MovieId,
                MovieTitle = m.Title,
                TotalTicketsSold = m.Screenings
                    .Where(s => s.Status != ScreeningStatus.Cancelled)
                    .SelectMany(s => s.TicketSales)
                    .Sum(t => (int?)t.TotalTickets) ?? 0,
                TotalRevenue = m.Screenings
                    .Where(s => s.Status != ScreeningStatus.Cancelled)
                    .SelectMany(s => s.TicketSales.Select(t => t.TotalTickets * s.TicketPrice))
                    .Sum(x => (decimal?)x) ?? 0m
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ThenBy(x => x.MovieTitle)
            .ToListAsync();

        return Ok(result);
    }

    // ---------- Hjælpemetoder ----------

    private async Task<int> TicketsSoldFor(int screeningId) =>
        await _db.TicketSales
            .Where(t => t.ScreeningId == screeningId)
            .SumAsync(t => (int?)t.TotalTickets) ?? 0;

    // Projektion direkte i SQL: kun de kolonner DTO'en skal bruge hentes
    private static readonly System.Linq.Expressions.Expression<Func<TicketSale, TicketSaleDto>> ToDtoProjection = t => new TicketSaleDto
    {
        TicketSaleId = t.TicketSaleId,
        SaleDate = t.SaleDate,
        TotalTickets = t.TotalTickets,
        ScreeningId = t.ScreeningId,
        ScreeningStartTime = t.Screening.StartTime,
        MovieTitle = t.Screening.Movie.Title,
        CinemaName = t.Screening.Screen.Cinema.Name,
        TicketPrice = t.Screening.TicketPrice,
        TotalPrice = t.TotalTickets * t.Screening.TicketPrice
    };
}
