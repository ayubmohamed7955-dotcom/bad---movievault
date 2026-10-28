using System.ComponentModel.DataAnnotations;
using MovieVault.Api.Entities;

namespace MovieVault.Api.DTOs;

// Bruges når en film oprettes (POST). Kun Title er påkrævet,
// fordi film må oprettes før alt om produktionen er kendt.
public class CreateMovieDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public DateOnly? ReleaseDate { get; set; }

    [Range(1, 1000, ErrorMessage = "Runtime must be between 1 and 1000 minutes.")]
    public int? RuntimeMinutes { get; set; }

    [MaxLength(10)]
    public string? AgeRating { get; set; }

    // "Planned", "InProduction", "Released" eller "Archived"
    [EnumDataType(typeof(MovieStatus))]
    public MovieStatus Status { get; set; } = MovieStatus.Planned;

    [Range(1, int.MaxValue, ErrorMessage = "DirectorId must be a valid id.")]
    public int? DirectorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ProductionCompanyId must be a valid id.")]
    public int? ProductionCompanyId { get; set; }
}

// Bruges når en film opdateres (PUT). Samme felter som ved oprettelse.
public class UpdateMovieDto : CreateMovieDto
{
}

// Det API'et returnerer for en film (CRUD)
public class MovieDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly? ReleaseDate { get; set; }
    public int? RuntimeMinutes { get; set; }
    public string? AgeRating { get; set; }
    public string Status { get; set; } = string.Empty;

    public int? DirectorId { get; set; }
    public string? DirectorName { get; set; }

    public int? ProductionCompanyId { get; set; }
    public string? ProductionCompanyName { get; set; }
}

// Query 1: Movie overview
public class MovieOverviewDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly? ReleaseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DirectorName { get; set; }
    public string? ProductionCompanyName { get; set; }
    // Fra ProductionRecord - null hvis filmen endnu ikke har et record
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
    public string Status { get; set; } = string.Empty;

    public string? DirectorName { get; set; }
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
    public string RoleType { get; set; } = string.Empty;
}

// En optagelseslokation for en film (inkl. ShootingDays fra MovieLocation)
public class FilmingLocationDto
{
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    public string LocationType { get; set; } = string.Empty;
    public int? ShootingDays { get; set; }
}
