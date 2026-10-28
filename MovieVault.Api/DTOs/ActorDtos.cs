using System.ComponentModel.DataAnnotations;
using MovieVault.Api.Entities;

namespace MovieVault.Api.DTOs;

// Tilføj en skuespiller til en films cast, fx POST /api/movies/{movieId}/cast
public class AddActorToMovieDto
{
    [Range(1, int.MaxValue, ErrorMessage = "ActorId must be a valid id.")]
    public int ActorId { get; set; }

    [Required(ErrorMessage = "CharacterName is required.")]
    [MaxLength(100)]
    public string CharacterName { get; set; } = string.Empty;

    // "Lead", "Supporting" eller "Cameo"
    [EnumDataType(typeof(RoleType))]
    public RoleType RoleType { get; set; }
}

// Query 5: Actor movie performance
public class ActorPerformanceDto
{
    public int ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;

    public List<ActorMoviePerformanceDto> Movies { get; set; } = new();

    // Summer på tværs af alle filmene
    public int TotalTicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class ActorMoviePerformanceDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public string RoleType { get; set; } = string.Empty;
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
}
