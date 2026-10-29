using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Data;
using MovieVault.Api.DTOs;
using MovieVault.Api.Entities;

namespace MovieVault.Api.Controllers;

// Filming locations er en under-ressource af en film /api/movies/{movieId}/locations
// Samme mønster som MovieCastController, men på junction-tabellen MovieLocation
[ApiController]
[Route("api/movies/{movieId:int}/locations")]
public class MovieLocationsController : ControllerBase
{
    private readonly MovieVaultDbContext _db;

    public MovieLocationsController(MovieVaultDbContext db)
    {
        _db = db;
    }
    // GET /api/movies/{movieId}/locations
    [HttpGet]
    public async Task<ActionResult<List<FilmingLocationDto>>> GetLocations(int movieId)
    {
        if (!await _db.Movies.AnyAsync(m => m.MovieId == movieId))
            return NotFound(new ProblemDetails { Title = $"Movie with id {movieId} was not found." });

        var locations = await _db.MovieLocations
            .AsNoTracking()
            .Where(ml => ml.MovieId == movieId)
            .OrderBy(ml => ml.Location.Name)
            .Select(ml => new FilmingLocationDto
            {
                LocationId = ml.LocationId,
                Name = ml.Location.Name,
                City = ml.Location.City,
                Country = ml.Location.Country,
                LocationType = ml.Location.LocationType,
                ShootingDays = ml.ShootingDays
            })
            .ToListAsync();

        return Ok(locations);
    }
    // POST /api/movies/{movieId}/locations tilknytter en location til filmen
    [HttpPost]
    public async Task<ActionResult<FilmingLocationDto>> AddLocation(int movieId, AddFilmingLocationDto dto)
    {
        if (!await _db.Movies.AnyAsync(m => m.MovieId == movieId))
            return NotFound(new ProblemDetails { Title = $"Movie with id {movieId} was not found." });

        var location = await _db.Locations.FindAsync(dto.LocationId);
        if (location is null)
        {
            ModelState.AddModelError(nameof(dto.LocationId), $"Location with id {dto.LocationId} does not exist.");
            return ValidationProblem(ModelState);
        }

        if (await _db.MovieLocations.AnyAsync(ml => ml.MovieId == movieId && ml.LocationId == dto.LocationId))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Location is already associated with the movie.",
                Detail = $"Location {dto.LocationId} is already linked to movie {movieId}."
            });
        }
        
        var movieLocation = new MovieLocation
        {
            MovieId = movieId,
            LocationId = dto.LocationId,
            ShootingDays = dto.ShootingDays
        };

        _db.MovieLocations.Add(movieLocation);
        await _db.SaveChangesAsync();

        var result = new FilmingLocationDto
        {
            LocationId = location.LocationId,
            Name = location.Name,
            City = location.City,
            Country = location.Country,
            LocationType = location.LocationType,
            ShootingDays = movieLocation.ShootingDays
        };
        return CreatedAtAction(nameof(GetLocations), new { movieId }, result);
    }
    
    // DELETE /api/movies/{movieId}/locations/{locationId}
    // Kun koblingen slettes, selve location bliver i databasen
    [HttpDelete("{locationId:int}")]
    public async Task<IActionResult> RemoveLocation(int movieId, int locationId)
    {
        var movieLocation = await _db.MovieLocations.FindAsync(movieId, locationId);
        if (movieLocation is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = $"Location {locationId} is not associated with movie {movieId}."
            });
        }

        _db.MovieLocations.Remove(movieLocation);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}