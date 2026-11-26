using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Data;
using MovieVault.Api.DTOs;

namespace MovieVault.Api.Controllers;

// Medier er seed-data - fuld CRUD er ikke et krav, så der er kun læse-endpoints.
// Brug GET /api/reviews?mediaOutletId={id} for at se et medies anmeldelser.
[ApiController]
[Route("api/media-outlets")]
[AllowAnonymous]
public class MediaOutletsController : ControllerBase
{
    private readonly MovieVaultDbContext _db;

    public MediaOutletsController(MovieVaultDbContext db)
    {
        _db = db;
    }

    /// <summary>Get all media outlets.</summary>
    /// <remarks>Public endpoint - no token required.</remarks>
    [HttpGet]
    public async Task<ActionResult<List<MediaOutletDto>>> GetAll()
    {
        var outlets = await _db.MediaOutlets
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new MediaOutletDto
            {
                MediaOutletId = o.MediaOutletId,
                Name = o.Name,
                OutletType = o.OutletType,
                Website = o.Website,
                ReviewCount = o.Reviews.Count
            })
            .ToListAsync();

        return Ok(outlets);
    }

    /// <summary>Get one media outlet by id.</summary>
    /// <remarks>Public endpoint - no token required.</remarks>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaOutletDto>> GetById(int id)
    {
        var outlet = await _db.MediaOutlets
            .AsNoTracking()
            .Where(o => o.MediaOutletId == id)
            .Select(o => new MediaOutletDto
            {
                MediaOutletId = o.MediaOutletId,
                Name = o.Name,
                OutletType = o.OutletType,
                Website = o.Website,
                ReviewCount = o.Reviews.Count
            })
            .FirstOrDefaultAsync();

        if (outlet is null)
            return NotFound(new ProblemDetails { Title = $"Media outlet with id {id} was not found." });

        return Ok(outlet);
    }
}
