namespace MovieVault.Api.Entities;

// Junction-entity for N:N filmed med attributten ShootingDays
// Primary key = (MovieId, LocationId)

public class MovieLocation
{
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public int? ShootingDays { get; set; }
}