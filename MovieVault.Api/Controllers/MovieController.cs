using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Data;
using MovieVault.Api.DTOs;
using MovieVault.Api.Entities;

namespace MovieVault.Api.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly MovieVaultDbContext _db;

    // Dependency injection af databasen som gives til controlleren
    public MoviesController(MovieVaultDbContext db)
    {
        _db = db;
    }

    // GET api/movies - 1. Movie overview
    [HttpGet]
     public async Task<ActionResult<List<MovieOverviewDto>>> GetAll()
    {
        // Select bliver til LEFT JOINs mod Director, ProductionCompany
        // og ProductionRecord i SQL. LEFT, fordi relationerne er optional.
        var movies = await _db.Movies
            .AsNoTracking()
            .OrderBy(m => m.Title)
            .Select(m => new MovieOverviewDto
            {
                MovieId = m.MovieId,
                Title = m.Title,
                ReleaseDate = m.ReleaseDate,
                Status = m.Status,
                DirectorName = m.Director != null ? m.Director.Name : null,
                ProductionCompanyName = m.ProductionCompany != null ? m.ProductionCompany.Name : null,
                Budget = m.ProductionRecord != null ? m.ProductionRecord.Budget : null
            })
            .ToListAsync();

        return Ok(movies);
    }

    // GET api/movies/{id} - 2. Movie details
        [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieDetailsDto>> GetById(int id)
    {
        var movie = await LoadDetailsAsync(id);
        if (movie is null)
            return NotFound(new ProblemDetails { Title = $"Movie with id {id} was not found." });

        return Ok(movie);
    }

    // POST api/movies
     [HttpPost]
    public async Task<ActionResult<MovieDetailsDto>> Create(MovieInputDto dto)
    {
        await ValidateBusinessRulesAsync(dto);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var movie = new Movie();
        MapToEntity(dto, movie);

        _db.Movies.Add(movie);
        await _db.SaveChangesAsync();

        var created = await LoadDetailsAsync(movie.MovieId);

        // 201 Created + Location-header, der peger på GET /api/movies/{id}
        return CreatedAtAction(nameof(GetById), new { id = movie.MovieId }, created);
    }

    // PUT api/movies/{id}
    // PUT erstatter hele filmen sammen med production record
        [HttpPut("{id:int}")]
    public async Task<ActionResult<MovieDetailsDto>> Update(int id, MovieInputDto dto)
    {
        var movie = await _db.Movies
            .Include(m => m.ProductionRecord)
            .FirstOrDefaultAsync(m => m.MovieId == id);

        if (movie is null)
            return NotFound(new ProblemDetails { Title = $"Movie with id {id} was not found." });

        await ValidateBusinessRulesAsync(dto);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        MapToEntity(dto, movie);
        await _db.SaveChangesAsync();

        return Ok(await LoadDetailsAsync(id));
    }

    // DELETE api/movies/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await _db.Movies.FindAsync(id);
        if (movie is null)
            return NotFound(new ProblemDetails { Title = $"Movie with id {id} was not found." });

        // Screenings har Restrict i DbContext. Vi tjekker selv først,
        // så klienten får en forståelig 409 i stedet for en databasefejl (500).
        var hasScreenings = await _db.Screenings.AnyAsync(s => s.MovieId == id);
        if (hasScreenings)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Movie cannot be deleted.",
                Detail = "The movie has screenings. Cancel or remove its screenings first."
            });
        }

        // Cast, filming locations og production record slettes automatisk
        _db.Movies.Remove(movie);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // Hjælpemetoder
    
    // Regler, som DataAnnotations ikke kan tjekke, fordi de kræver databasen
    // eller sammenligner to felter
    private async Task ValidateBusinessRulesAsync(MovieInputDto dto)
    {
        if (dto.DirectorId is not null &&
            !await _db.Directors.AnyAsync(d => d.DirectorId == dto.DirectorId))
        {
            ModelState.AddModelError(nameof(dto.DirectorId), $"Director with id {dto.DirectorId} does not exist.");
        }

        if (dto.ProductionCompanyId is not null &&
            !await _db.ProductionCompanies.AnyAsync(pc => pc.ProductionCompanyId == dto.ProductionCompanyId))
        {
            ModelState.AddModelError(nameof(dto.ProductionCompanyId), $"Production company with id {dto.ProductionCompanyId} does not exist.");
        }

        var pr = dto.ProductionRecord;
        if (pr?.ShootingStartDate is not null && pr.ShootingEndDate is not null &&
            pr.ShootingEndDate < pr.ShootingStartDate)
        {
            ModelState.AddModelError("ProductionRecord.ShootingEndDate", "Shooting end date cannot be before the start date.");
        }
    }

    // kopierer værdier fra DTO til entity, som bruges af både POST og PUT
    private static void MapToEntity(MovieInputDto dto, Movie movie)
    {
        movie.Title = dto.Title;
        movie.ReleaseDate = dto.ReleaseDate;
        movie.RuntimeMinutes = dto.RuntimeMinutes;
        movie.AgeRating = dto.AgeRating;
        movie.Status = dto.Status;
        movie.DirectorId = dto.DirectorId;
        movie.ProductionCompanyId = dto.ProductionCompanyId;

        if (dto.ProductionRecord is null)
        {
            // Ingen produktionsinfo -> ingen record (EF sletter den eksisterende)
            movie.ProductionRecord = null;
        }
        else
        {
            movie.ProductionRecord ??= new ProductionRecord();
            movie.ProductionRecord.Budget = dto.ProductionRecord.Budget;
            movie.ProductionRecord.ShootingStartDate = dto.ProductionRecord.ShootingStartDate;
            movie.ProductionRecord.ShootingEndDate = dto.ProductionRecord.ShootingEndDate;
        }
    }

    // Henter en film med alle relationer
    // Cast og FilmingLocations går gennem junction-tabellerne
     private Task<MovieDetailsDto?> LoadDetailsAsync(int id)
    {
        return _db.Movies
            .AsNoTracking()
            .Where(m => m.MovieId == id)
            .Select(m => new MovieDetailsDto
            {
                MovieId = m.MovieId,
                Title = m.Title,
                ReleaseDate = m.ReleaseDate,
                RuntimeMinutes = m.RuntimeMinutes,
                AgeRating = m.AgeRating,
                Status = m.Status,

                DirectorId = m.DirectorId,
                DirectorName = m.Director != null ? m.Director.Name : null,

                ProductionCompanyId = m.ProductionCompanyId,
                ProductionCompanyName = m.ProductionCompany != null ? m.ProductionCompany.Name : null,

                Budget = m.ProductionRecord != null ? m.ProductionRecord.Budget : null,
                ShootingStartDate = m.ProductionRecord != null ? m.ProductionRecord.ShootingStartDate : null,
                ShootingEndDate = m.ProductionRecord != null ? m.ProductionRecord.ShootingEndDate : null,

                Cast = m.Cast
                    .OrderBy(c => c.Actor.Name)
                    .Select(c => new CastMemberDto
                    {
                        ActorId = c.ActorId,
                        ActorName = c.Actor.Name,
                        CharacterName = c.CharacterName,
                        RoleType = c.RoleType
                    })
                    .ToList(),

                FilmingLocations = m.FilmingLocations
                    .Select(fl => new FilmingLocationDto
                    {
                        LocationId = fl.LocationId,
                        Name = fl.Location.Name,
                        City = fl.Location.City,
                        Country = fl.Location.Country,
                        LocationType = fl.Location.LocationType,
                        ShootingDays = fl.ShootingDays
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }
}

