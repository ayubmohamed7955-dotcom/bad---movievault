using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Data;
using MovieVault.Api.DTOs;
using MovieVault.Api.Entities;

namespace MovieVault.Api.Controllers;

// Cast er en under-ressource af en film: /api/movies/{movieId}/cast
// Hver handling arbejder på junction-tabellen MovieActor

[ApiController]
[Route("api/movies/{movieId:int}/cast")]

public class MovieCastController : ControllerBase
{
    private readonly MovieVaultDbContext _db;

    public MovieCastController(MovieVaultDbContext db)
    {
        _db = db;
    }
    // GET /api/movies/{movieId}/cast
    [HttpGet]
    public async Task<ActionResult<List<CastMemberDto>>> GetCast(int movieId)
    {
        if (!await _db.Movies.AnyAsync(m => m.MovieId == movieId))
            return NotFound(new ProblemDetails { Title = $"Movie with id {movieId} was not found." });

        var cast = await _db.MovieActors
            .AsNoTracking()
            .Where(ma => ma.MovieId == movieId)
            .OrderBy(ma => ma.Actor.Name)
            .Select(ma => new CastMemberDto
            {
                ActorId = ma.ActorId,
                ActorName = ma.Actor.Name,
                CharacterName = ma.CharacterName,
                RoleType = ma.RoleType
            })
            .ToListAsync();

        return Ok(cast);
    }
    // POST /api/movies/{movieId}/cast tilføjer en skuespiller til filmen
    [HttpPost]
    public async Task<ActionResult<CastMemberDto>> AddActor(int movieId, AddCastMemberDto dto)
    {
        // Filmen står i URL'en, findes den ikke, er ressourcen væk: 404
        if (!await _db.Movies.AnyAsync(m => m.MovieId == movieId))
            return NotFound(new ProblemDetails { Title = $"Movie with id {movieId} was not found." });

        // Skuespilleren står i body, findes den ikke, er input ugyldigt: 400
        var actor = await _db.Actors.FindAsync(dto.ActorId);
        if (actor is null)
        {
            ModelState.AddModelError(nameof(dto.ActorId), $"Actor with id {dto.ActorId} does not exist.");
            return ValidationProblem(ModelState);
        }

        // Den sammensatte primary key (MovieId, ActorId) tillader kun én rolle per skuespiller per film
        // Vi tjekker selv først, så klienten får 409 i stedet for en databasefejl (500)
        if (await _db.MovieActors.AnyAsync(ma => ma.MovieId == movieId && ma.ActorId == dto.ActorId))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Actor is already in the cast.",
                Detail = $"Actor {dto.ActorId} is already assigned to movie {movieId}."
            });
        }
        var movieActor = new MovieActor
        {
            MovieId = movieId,
            ActorId = dto.ActorId,
            CharacterName = dto.CharacterName,
            RoleType = dto.RoleType
        };

        _db.MovieActors.Add(movieActor);
        await _db.SaveChangesAsync();

        var result = new CastMemberDto
        {
            ActorId = actor.ActorId,
            ActorName = actor.Name,
            CharacterName = movieActor.CharacterName,
            RoleType = movieActor.RoleType
        };

        return CreatedAtAction(nameof(GetCast), new { movieId }, result);
    }
    // DELETE /api/movies/{movieId}/cast/{actorId} fjerner skuespilleren fra filmen
    // Kun rækken i MovieActor slettes, skuespilleren selv bliver i databasen
    [HttpDelete("{actorId:int}")]
    public async Task<IActionResult> RemoveActor(int movieId, int actorId)
    {
        var movieActor = await _db.MovieActors.FindAsync(movieId, actorId);
        if (movieActor is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = $"Actor {actorId} is not in the cast of movie {movieId}."
            });
        }

        _db.MovieActors.Remove(movieActor);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}