using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Data;
using MovieVault.Api.DTOs;
using MovieVault.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MovieVault.Api.Controllers;

[ApiController]
[Route("api/screenings")]
[Produces("application/json")]
[Authorize]
public class ScreeningsController : ControllerBase
{
    // Bruges hvis en film ikke har en runtime endnu, når vi tjekker om to screenings overlapper i samme sal
    private const int DefaultRuntimeMinutes = 120;

    // Pause mellem to screenings i samme sal (rengøring, ind- og udgang)
    private const int CleaningBufferMinutes = 15;

    private readonly MovieVaultDbContext _db;

    public ScreeningsController(MovieVaultDbContext db)
    {
        _db = db;
    }

    /// <summary>Henter én screening.</summary>

[HttpGet("{id:int}", Name = nameof(GetScreening))]
[ProducesResponseType(typeof(ScreeningDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<ScreeningDto>> GetScreening(int id)
{
    // Find den ønskede screening
    var screening = await ScreeningsWithDetails()
        .FirstOrDefaultAsync(s => s.ScreeningId == id);

    if (screening is null)
    {
        return Problem(
            statusCode: 404,
            title: "Screening not found",
            detail: $"No screening with id {id}.");
    }

    // Find brugerens ID fra JWT
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Admin og Reviewer må se alle screenings
    if (User.IsInRole("Admin") || User.IsInRole("Reviewer"))
    {
        return Ok(ToDto(screening));
    }

    // Actor må kun se screenings for egne film
    if (User.IsInRole("Actor"))
    {
        var hasAccess = await _db.MovieActors.AnyAsync(ma =>
            ma.MovieId == screening.MovieId &&
            ma.Actor.UserId == userId);

        if (!hasAccess)
            return Forbid();
    }
    // Director må kun se screenings for egne film
    else if (User.IsInRole("Director"))
    {
        var hasAccess = await _db.Movies.AnyAsync(m =>
            m.MovieId == screening.MovieId &&
            m.Director != null &&
            m.Director.UserId == userId);

        if (!hasAccess)
            return Forbid();
    }
    else
    {
        return Forbid();
    }

    return Ok(ToDto(screening));
}


    /// <summary>Henter alle screenings for en film, sorteret efter tidspunkt.</summary>

[HttpGet("~/api/movies/{movieId:int}/screenings")]
[ProducesResponseType(
    typeof(IEnumerable<ScreeningDto>),
    StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<IEnumerable<ScreeningDto>>>
    GetScreeningsForMovie(int movieId)
{
    // Kontrollér først om filmen eksisterer
    if (!await _db.Movies.AnyAsync(m => m.MovieId == movieId))
    {
        return Problem(
            statusCode: 404,
            title: "Movie not found",
            detail: $"No movie with id {movieId}.");
    }

    // Find den indloggede brugers ID
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Admin og Reviewer må se alle film
    if (User.IsInRole("Admin") || User.IsInRole("Reviewer"))
    {
        // Ingen begrænsning
    }
    else if (User.IsInRole("Actor"))
    {
        // Kontrollér om Actor medvirker i filmen
        var hasAccess = await _db.MovieActors.AnyAsync(ma =>
            ma.MovieId == movieId &&
            ma.Actor.UserId == userId);

        if (!hasAccess)
            return Forbid();
    }
    else if (User.IsInRole("Director"))
    {
        // Kontrollér om Director har instrueret filmen
        var hasAccess = await _db.Movies.AnyAsync(m =>
            m.MovieId == movieId &&
            m.Director != null &&
            m.Director.UserId == userId);

        if (!hasAccess)
            return Forbid();
    }
    else
    {
        return Forbid();
    }

    // Hent forestillinger efter adgangskontrol
    var screenings = await ScreeningsWithDetails()
        .Where(s => s.MovieId == movieId)
        .OrderBy(s => s.StartTime)
        .ToListAsync();

    return Ok(screenings.Select(ToDto));
}


    /// <summary>Henter alle screenings i en biograf (på tværs af dens sale), sorteret efter tidspunkt.</summary>
[HttpGet("~/api/cinemas/{cinemaId:int}/screenings")]
[ProducesResponseType(
    typeof(IEnumerable<ScreeningDto>),
    StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<IEnumerable<ScreeningDto>>>
    GetScreeningsForCinema(int cinemaId)
{
    // Kontrollér, at biografen eksisterer
    if (!await _db.Cinemas.AnyAsync(
        c => c.CinemaId == cinemaId))
    {
        return Problem(
            statusCode: 404,
            title: "Cinema not found",
            detail: $"No cinema with id {cinemaId}.");
    }

    // Find den indloggede bruger
    var userId = User.FindFirstValue(
        ClaimTypes.NameIdentifier);

    // Start med alle screenings i biografen
    var query = ScreeningsWithDetails()
        .Where(s => s.Screen.CinemaId == cinemaId);

    // Admin må se alle screenings
    if (User.IsInRole("Admin"))
    {
        // Ingen filtrering
    }
    else if (User.IsInRole("Actor"))
    {
        // Kun screenings for film, Actor medvirker i
        query = query.Where(s =>
            s.Movie.Cast.Any(ma =>
                ma.Actor.UserId == userId));
    }
    else if (User.IsInRole("Director"))
    {
        // Kun screenings for film, Director har instrueret
        query = query.Where(s =>
            s.Movie.Director != null &&
            s.Movie.Director.UserId == userId);
    }
    else if (!User.IsInRole("Reviewer"))
    {
        return Forbid();
    }

    // Hent kun de tilladte screenings
    var screenings = await query
        .OrderBy(s => s.StartTime)
        .ToListAsync();

    return Ok(screenings.Select(ToDto));
}


 
[HttpGet("upcoming")]
[ProducesResponseType(
    typeof(IEnumerable<UpcomingScreeningDto>),
    StatusCodes.Status200OK)]
public async Task<ActionResult<IEnumerable<UpcomingScreeningDto>>>
    GetUpcomingScreenings()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var now = DateTime.Now;

    // Start med alle kommende forestillinger
    var query = _db.Screenings
        .AsNoTracking()
        .Where(s => s.StartTime > now &&
                    s.Status == ScreeningStatus.Scheduled);

    // Actor ser kun forestillinger for egne film
    if (User.IsInRole("Admin"))
    {
        // Admin ser alle
    }
    else if (User.IsInRole("Actor"))
    {
        query = query.Where(s =>
            s.Movie.Cast.Any(ma =>
                ma.Actor.UserId == userId));
    }
    // Director ser kun forestillinger for egne film
    else if (User.IsInRole("Director"))
    {
        query = query.Where(s =>
            s.Movie.Director != null &&
            s.Movie.Director.UserId == userId);
    }
    else if (!User.IsInRole("Reviewer"))
    {
        return Forbid();
    }

    var upcoming = await query
        .OrderBy(s => s.StartTime)
        .Select(s => new UpcomingScreeningDto
        {
            ScreeningId = s.ScreeningId,
            MovieTitle = s.Movie.Title,
            CinemaName = s.Screen.Cinema.Name,
            City = s.Screen.Cinema.City,
            ScreenNumber = s.Screen.ScreenNumber,
            StartTime = s.StartTime,
            TicketPrice = s.TicketPrice
        })
        .ToListAsync();

    return Ok(upcoming);
}


    /// <summary>Opretter en screening for en film i en sal.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ScreeningDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ScreeningDto>> CreateScreening(CreateScreeningDto dto)
    {
        var startTime = dto.StartTime!.Value; // [Required] på DTO'en sikrer at den ikke er null

        var movie = await _db.Movies.FindAsync(dto.MovieId);
        if (movie is null)
            ModelState.AddModelError(nameof(dto.MovieId), $"No movie with id {dto.MovieId}.");

        if (!await _db.Screens.AnyAsync(s => s.ScreenId == dto.ScreenId))
            ModelState.AddModelError(nameof(dto.ScreenId), $"No screen with id {dto.ScreenId}.");

        if (startTime <= DateTime.Now)
            ModelState.AddModelError(nameof(dto.StartTime), "A new screening must start in the future.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var runtime = movie!.RuntimeMinutes ?? DefaultRuntimeMinutes;
        var clash = await FindOverlappingScreening(dto.ScreenId, startTime, runtime, excludeScreeningId: null);
        if (clash is not null)
        {
            ModelState.AddModelError(nameof(dto.StartTime),
                $"Screen is already booked by screening {clash.ScreeningId} at {clash.StartTime:yyyy-MM-dd HH:mm}.");
            return ValidationProblem(ModelState);
        }

        var screening = new Screening
        {
            MovieId = dto.MovieId,
            ScreenId = dto.ScreenId,
            StartTime = startTime,
            TicketPrice = dto.TicketPrice,
            Status = ScreeningStatus.Scheduled
        };

        _db.Screenings.Add(screening);
        await _db.SaveChangesAsync();

        var created = await ScreeningsWithDetails().FirstAsync(s => s.ScreeningId == screening.ScreeningId);
        return CreatedAtRoute(nameof(GetScreening), new { id = screening.ScreeningId }, ToDto(created));
    }

    /// <summary>
    /// Opdaterer en screening (sal, tidspunkt, pris) eller annullerer den ved at sætte Status = "Cancelled".
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ScreeningDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin")]

    public async Task<ActionResult<ScreeningDto>> UpdateScreening(int id, UpdateScreeningDto dto)
    {
        var screening = await _db.Screenings
            .Include(s => s.Movie)
            .FirstOrDefaultAsync(s => s.ScreeningId == id);
        if (screening is null)
            return Problem(statusCode: 404, title: "Screening not found", detail: $"No screening with id {id}.");

        var startTime = dto.StartTime!.Value;

        var newScreen = await _db.Screens.FindAsync(dto.ScreenId);
        if (newScreen is null)
        {
            ModelState.AddModelError(nameof(dto.ScreenId), $"No screen with id {dto.ScreenId}.");
            return ValidationProblem(ModelState);
        }

        // Flyttes screeningen, skal det nye tidspunkt ligge i fremtiden
        if (startTime != screening.StartTime && startTime <= DateTime.Now)
            ModelState.AddModelError(nameof(dto.StartTime), "A screening can only be moved to a time in the future.");

        // Flyttes screeningen til en mindre sal, skal der stadig være plads til de solgte billetter
        var ticketsSold = await _db.TicketSales.Where(t => t.ScreeningId == id).SumAsync(t => (int?)t.TotalTickets) ?? 0;
        if (ticketsSold > newScreen.SeatingCapacity)
            ModelState.AddModelError(nameof(dto.ScreenId),
                $"Screen {newScreen.ScreenId} only has {newScreen.SeatingCapacity} seats, but {ticketsSold} tickets are already sold.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // En aktiv screening må ikke overlappe en anden i samme sal
        if (dto.Status == ScreeningStatus.Scheduled)
        {
            var runtime = screening.Movie.RuntimeMinutes ?? DefaultRuntimeMinutes;
            var clash = await FindOverlappingScreening(dto.ScreenId, startTime, runtime, excludeScreeningId: id);
            if (clash is not null)
            {
                ModelState.AddModelError(nameof(dto.StartTime),
                    $"Screen is already booked by screening {clash.ScreeningId} at {clash.StartTime:yyyy-MM-dd HH:mm}.");
                return ValidationProblem(ModelState);
            }
        }

        screening.ScreenId = dto.ScreenId;
        screening.StartTime = startTime;
        screening.TicketPrice = dto.TicketPrice;
        screening.Status = dto.Status;

        await _db.SaveChangesAsync();

        var updated = await ScreeningsWithDetails().FirstAsync(s => s.ScreeningId == id);
        return Ok(ToDto(updated));
    }

    /// <summary>
    /// Sletter en screening. Kun muligt hvis der ikke er solgt billetter - ellers skal den annulleres i stedet.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteScreening(int id)
    {
        var screening = await _db.Screenings.FindAsync(id);
        if (screening is null)
            return Problem(statusCode: 404, title: "Screening not found", detail: $"No screening with id {id}.");

        if (await _db.TicketSales.AnyAsync(t => t.ScreeningId == id))
            return Problem(statusCode: 409, title: "Screening has ticket sales",
                detail: "Screenings with ticket sales cannot be deleted. Cancel it instead (PUT with Status = \"Cancelled\").");

        _db.Screenings.Remove(screening);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------- Hjælpemetoder ----------

    // Screening med alt hvad ScreeningDto skal bruge: film, sal, biograf og billetsalg
    private IQueryable<Screening> ScreeningsWithDetails() =>
        _db.Screenings
            .AsNoTracking()
            .Include(s => s.Movie)
            .Include(s => s.Screen).ThenInclude(sc => sc.Cinema)
            .Include(s => s.TicketSales);

    private static ScreeningDto ToDto(Screening s)
    {
        var sold = s.TicketSales.Sum(t => t.TotalTickets);
        return new ScreeningDto
        {
            ScreeningId = s.ScreeningId,
            StartTime = s.StartTime,
            TicketPrice = s.TicketPrice,
            Status = s.Status.ToString(),
            MovieId = s.MovieId,
            MovieTitle = s.Movie.Title,
            ScreenId = s.ScreenId,
            ScreenNumber = s.Screen.ScreenNumber,
            SeatingCapacity = s.Screen.SeatingCapacity,
            CinemaId = s.Screen.CinemaId,
            CinemaName = s.Screen.Cinema.Name,
            TicketsSold = sold,
            AvailableSeats = s.Screen.SeatingCapacity - sold
        };
    }

    // Finder en aktiv screening i samme sal, hvis tidsrum overlapper det nye (inkl. rengøringspause)
    private async Task<Screening?> FindOverlappingScreening(int screenId, DateTime start, int runtimeMinutes, int? excludeScreeningId)
    {
        var end = start.AddMinutes(runtimeMinutes + CleaningBufferMinutes);

        // Hent kun kandidater fra døgnet omkring, og lav selve overlap-tjekket i C#
        var candidates = await _db.Screenings
            .AsNoTracking()
            .Include(s => s.Movie)
            .Where(s => s.ScreenId == screenId
                        && s.Status == ScreeningStatus.Scheduled
                        && s.ScreeningId != excludeScreeningId
                        && s.StartTime > start.AddDays(-1)
                        && s.StartTime < end)
            .ToListAsync();

        return candidates.FirstOrDefault(s =>
        {
            var otherEnd = s.StartTime.AddMinutes((s.Movie.RuntimeMinutes ?? DefaultRuntimeMinutes) + CleaningBufferMinutes);
            return s.StartTime < end && start < otherEnd;
        });
    }
}
