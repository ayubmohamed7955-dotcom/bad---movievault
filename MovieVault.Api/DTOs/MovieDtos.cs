using System.ComponentModel.DataAnnotations;
using MovieVault.Api.Entities;

namespace MovieVault.Api.DTOs;

// Request DTO

// Produktionsinfo er valgfrit, så en film kan oprettes før den kendes
public class ProductionInputDto
{
    [Range(typeof(decimal), "0", "1000000000000", ErrorMessage = "Budget cannot be negative.")]
    public decimal? Budget { get; set; }

    public DateOnly? ShootingStartDate { get; set; }
    public DateOnly? ShootingEndDate { get; set; }
}

// Bruges til både POST og PUT
public class MovieInputDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title can be at most 200 characters.")]
    public string Title { get; set; } = string.Empty;

    public DateOnly? ReleaseDate { get; set; }

    [Range(1, 600, ErrorMessage = "Runtime must be between 1 and 600 minutes.")]
    public int? RuntimeMinutes { get; set; }

    [StringLength(10, ErrorMessage = "Age rating can be at most 10 characters.")]
    public string? AgeRating { get; set; }

    // "Planned", "InProduction", "Released" eller "Archived"
    // Udelades feltet bliver filmen til "Planned"
    [EnumDataType(typeof(MovieStatus), ErrorMessage = "Invalid status.")]
    public MovieStatus Status { get; set; } = MovieStatus.Planned;

    [Range(1, int.MaxValue, ErrorMessage = "DirectorId must be a valid id.")]
    public int? DirectorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ProductionCompanyId must be a valid id.")]
    public int? ProductionCompanyId { get; set; }
    public ProductionInputDto? ProductionRecord { get; set; }
}

// Det API'et returnerer for en film (CRUD)
public class MovieOverviewDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly? ReleaseDate { get; set; }
    public MovieStatus Status { get; set; }
    public string? DirectorName { get; set; }
    public string? ProductionCompanyName { get; set; }
    public decimal? Budget { get; set; }
}

// Query 2: Movie details by id
public class MovieDetailsDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly? ReleaseDate { get; set; }
    public int? RuntimeMinutes { get; set; }
    public string? AgeRating { get; set; }
    public MovieStatus Status { get; set; }
    public int? DirectorId { get; set; }
    public string? DirectorName { get; set; }
    public int? ProductionCompanyId { get; set; }
    public string? ProductionCompanyName { get; set; }

    public decimal? Budget { get; set; }
    public DateOnly? ShootingStartDate { get; set; }
    public DateOnly? ShootingEndDate { get; set; }

    public List<CastMemberDto> Cast { get; set; } = new();
    public List<FilmingLocationDto> FilmingLocations { get; set; } = new();
}

// En skuespiller i en films cast (inkl. rolle-information fra MovieActor)
public class CastMemberDto
{
    public int ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public RoleType RoleType { get; set; }
}

// En optagelseslokation for en film (inkl. ShootingDays fra MovieLocation)
public class FilmingLocationDto
{
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    public LocationType LocationType { get; set; }
    public int? ShootingDays { get; set; }
}
