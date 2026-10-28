using System.ComponentModel.DataAnnotations;

namespace MovieVault.Api.DTOs;

// Tilknyt en optagelseslokation til en film, fx POST /api/movies/{movieId}/locations
public class AddLocationToMovieDto
{
    [Range(1, int.MaxValue, ErrorMessage = "LocationId must be a valid id.")]
    public int LocationId { get; set; }

    [Range(1, 1000, ErrorMessage = "ShootingDays must be at least 1.")]
    public int? ShootingDays { get; set; }
}
