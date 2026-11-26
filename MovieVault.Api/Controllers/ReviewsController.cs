using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Data;
using MovieVault.Api.DTOs;
using MovieVault.Api.Entities;

namespace MovieVault.Api.Controllers;

// Læsning er offentlig ([AllowAnonymous]).
// Oprettelse, ændring og sletning kræver rollen Reviewer (kun egne anmeldelser) eller Admin (alle).
[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly MovieVaultDbContext _db;

    public ReviewsController(MovieVaultDbContext db)
    {
        _db = db;
    }

    /// <summary>Get reviews, optionally filtered by movie and/or media outlet.</summary>
    /// <remarks>
    /// Public endpoint - no token required.
    /// Filtering uses query parameters, which can be combined:
    /// `GET /api/reviews?movieId=1` (all reviews of a movie),
    /// `GET /api/reviews?mediaOutletId=1` (all reviews from a media outlet),
    /// `GET /api/reviews?movieId=1&amp;mediaOutletId=1`.
    /// Returns 404 if the given movie or media outlet does not exist, and 200 with an empty list
    /// if it exists but has no reviews.
    /// </remarks>
    /// <param name="movieId">Only reviews of this movie.</param>
    /// <param name="mediaOutletId">Only reviews published by this media outlet.</param>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReviewDto>>> GetAll([FromQuery] int? movieId, [FromQuery] int? mediaOutletId)
    {
        var query = _db.Reviews.AsNoTracking().AsQueryable();

        if (movieId is not null)
        {
            if (!await _db.Movies.AnyAsync(m => m.MovieId == movieId))
                return NotFound(new ProblemDetails { Title = $"Movie with id {movieId} was not found." });

            query = query.Where(r => r.MovieId == movieId);
        }

        if (mediaOutletId is not null)
        {
            if (!await _db.MediaOutlets.AnyAsync(o => o.MediaOutletId == mediaOutletId))
                return NotFound(new ProblemDetails { Title = $"Media outlet with id {mediaOutletId} was not found." });

            query = query.Where(r => r.MediaOutletId == mediaOutletId);
        }

        var reviews = await query
            .OrderByDescending(r => r.PublishedAt)
            .Select(ToDto)
            .ToListAsync();

        return Ok(reviews);
    }

    /// <summary>Get one review by id.</summary>
    /// <remarks>Public endpoint - no token required.</remarks>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> GetById(int id)
    {
        var review = await LoadDtoAsync(id);

        if (review is null)
            return NotFound(new ProblemDetails { Title = $"Review with id {id} was not found." });

        return Ok(review);
    }

    /// <summary>Create a review.</summary>
    /// <remarks>
    /// Reviewer: the author is the logged-in reviewer and the media outlet is the outlet the reviewer represents.
    /// `reviewerId` can be omitted; sending another reviewer's id returns 403.
    /// Admin: must send `reviewerId` (unless the Admin also has a reviewer profile).
    /// The publication date is set by the server (UTC).
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Reviewer,Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReviewDto>> Create(CreateReviewDto dto)
    {
        Reviewer? author;

        if (User.IsInRole("Admin"))
        {
            // Admin vælger forfatteren - eller bruger sin egen anmelderprofil, hvis den findes
            author = dto.ReviewerId is not null
                ? await _db.Reviewers.FindAsync(dto.ReviewerId)
                : await GetCurrentReviewerAsync();

            if (author is null)
            {
                ModelState.AddModelError(nameof(dto.ReviewerId), dto.ReviewerId is null
                    ? "ReviewerId is required when an Admin creates a review."
                    : $"Reviewer with id {dto.ReviewerId} does not exist.");
            }
        }
        else
        {
            // Reviewer: forfatteren findes ALTID ud fra tokenet, aldrig ud fra klientens data
            author = await GetCurrentReviewerAsync();

            // Brugeren har rollen, men ingen anmelderprofil -> må ikke skrive anmeldelser
            if (author is null)
                return Forbid();

            // Forsøg på at skrive på vegne af en anden anmelder
            if (dto.ReviewerId is not null && dto.ReviewerId != author.ReviewerId)
                return Forbid();
        }

        if (!await _db.Movies.AnyAsync(m => m.MovieId == dto.MovieId))
            ModelState.AddModelError(nameof(dto.MovieId), $"Movie with id {dto.MovieId} does not exist.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var review = new Review
        {
            MovieId = dto.MovieId,
            Headline = dto.Headline,
            Body = dto.Body,
            Rating = dto.Rating,
            PublishedAt = DateTime.UtcNow,
            ReviewerId = author!.ReviewerId,
            // Anmeldelsen udgives i det medie, anmelderen repræsenterer
            MediaOutletId = author.MediaOutletId
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        var created = await LoadDtoAsync(review.ReviewId);

        return CreatedAtAction(nameof(GetById), new { id = review.ReviewId }, created);
    }

    /// <summary>Update a review (headline, text and rating).</summary>
    /// <remarks>Reviewer: own reviews only (403 otherwise). Admin: any review.</remarks>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Reviewer,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> Update(int id, UpdateReviewDto dto)
    {
        var review = await _db.Reviews.FindAsync(id);

        if (review is null)
            return NotFound(new ProblemDetails { Title = $"Review with id {id} was not found." });

        if (!await CanManageAsync(review))
            return Forbid();

        review.Headline = dto.Headline;
        review.Body = dto.Body;
        review.Rating = dto.Rating;

        await _db.SaveChangesAsync();

        return Ok(await LoadDtoAsync(id));
    }

    /// <summary>Delete a review.</summary>
    /// <remarks>Reviewer: own reviews only (403 otherwise). Admin: any review.</remarks>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Reviewer,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _db.Reviews.FindAsync(id);

        if (review is null)
            return NotFound(new ProblemDetails { Title = $"Review with id {id} was not found." });

        if (!await CanManageAsync(review))
            return Forbid();

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // Hjælpemetoder

    // Finder anmelderprofilen for den indloggede bruger via UserId i JWT'en
    private async Task<Reviewer?> GetCurrentReviewerAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return null;

        return await _db.Reviewers.FirstOrDefaultAsync(r => r.UserId == userId);
    }

    // Ejerskabstjek: Admin må alt, en Reviewer kun sine egne anmeldelser
    private async Task<bool> CanManageAsync(Review review)
    {
        if (User.IsInRole("Admin"))
            return true;

        var current = await GetCurrentReviewerAsync();

        return current is not null && review.ReviewerId == current.ReviewerId;
    }

    private Task<ReviewDto?> LoadDtoAsync(int id)
    {
        return _db.Reviews
            .AsNoTracking()
            .Where(r => r.ReviewId == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
    }

    // Projektion fra entity til DTO - oversættes af EF Core til én SQL-forespørgsel med JOINs
    private static readonly Expression<Func<Review, ReviewDto>> ToDto = r => new ReviewDto
    {
        ReviewId = r.ReviewId,
        Headline = r.Headline,
        Body = r.Body,
        Rating = r.Rating,
        PublishedAt = r.PublishedAt,
        MovieId = r.MovieId,
        MovieTitle = r.Movie.Title,
        ReviewerId = r.ReviewerId,
        ReviewerName = r.Reviewer.Name,
        MediaOutletId = r.MediaOutletId,
        MediaOutletName = r.MediaOutlet.Name,
        MediaOutletType = r.MediaOutlet.OutletType
    };
}
