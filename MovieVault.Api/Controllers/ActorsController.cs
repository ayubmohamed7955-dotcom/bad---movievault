using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Data;
using MovieVault.Api.DTOs;
using MovieVault.Api.Entities;

namespace MovieVault.Api.Controllers;

[ApiController]
[Route("api/actors")]
public class ActorsController : ControllerBase
{
    private readonly MovieVaultDbContext _db;

    public ActorsController(MovieVaultDbContext db)
    {
        _db = db;
    }

    // GET /api/actors
    [HttpGet]
    public async Task<ActionResult<List<ActorDto>>> GetAll()
    {
        var actors = await _db.Actors
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new ActorDto
            {
                ActorId = a.ActorId,
                Name = a.Name,
                Nationality = a.Nationality
            })
            .ToListAsync();

        return Ok(actors);
    }
    
    // GET /api/actors/{actorId}/performance
    // Længeste kæde: Actor -> MovieActor -> Movie -> Screening -> TicketSale
    [HttpGet("{actorId:int}/performance")]
    public async Task<ActionResult<ActorPerformanceDto>> GetPerformance(int actorId)
    {
        var performance = await _db.Actors
            .AsNoTracking()
            .Where(a => a.ActorId == actorId)
            .Select(a => new ActorPerformanceDto
            {
                ActorId = a.ActorId,
                ActorName = a.Name,

                // Roles = skuespillerens rækker i junction-tabellen MovieActor
                Movies = a.Roles
                    .Select(r => new ActorMoviePerformanceDto
                    {
                        MovieId = r.MovieId,
                        Title = r.Movie.Title,
                        CharacterName = r.CharacterName,
                        RoleType = r.RoleType,

                        // Alle filmens screenings (undtagen aflyste)
                        // -> SelectMany samler alle deres ticket sales i én liste
                        // -> Sum lægger billetterne sammen (SQL: SUM).
                        // (int?) + ?? 0 sikrer 0 i stedet for NULL, hvis filmen intet har solgt.
                        TicketsSold = r.Movie.Screenings
                            .Where(s => s.Status != ScreeningStatus.Cancelled)
                            .SelectMany(s => s.TicketSales)
                            .Sum(ts => (int?)ts.TotalTickets) ?? 0
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (performance is null)
            return NotFound(new ProblemDetails { Title = $"Actor with id {actorId} was not found." });

        // Totalerne beregnes ud fra listen, der allerede er hentet (SQL: COUNT og SUM)
        performance.Movies = performance.Movies
            .OrderByDescending(m => m.TicketsSold)
            .ToList();
        performance.NumberOfMovies = performance.Movies.Count;
        performance.TotalTicketsSold = performance.Movies.Sum(m => m.TicketsSold);

        return Ok(performance);
    }
}
