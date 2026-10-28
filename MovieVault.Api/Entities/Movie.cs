namespace MovieVault.Api.Entities;

public class Movie
{
    // Primary key
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly? ReleaseDate { get; set; }
    public int? RuntimeMinutes { get; set; }
    public string? AgeRating { get; set; }
    public MovieStatus Status { get; set; } = MovieStatus.Planned;

    // directs: optional - en film KAN være uden director -> int?
    public int? DirectorId { get; set; }
    public Director? Director { get; set; }

    // produces: optional - en film KAN være uden production company -> int?
    public int? ProductionCompanyId { get; set; }
    public ProductionCompany? ProductionCompany { get; set; }

    // has: 1:1 - FK (MovieId) ligger på ProductionRecord
    public ProductionRecord? ProductionRecord { get; set; }
    
    // N:N via junction-entities (features og filmed)
    public List<MovieActor> Cast { get; set; } = new();
    public List<MovieLocation> FilmingLocations { get; set; } = new();

    // shows: 1:N
    public List<Screening> Screenings { get; set; } = new();
}