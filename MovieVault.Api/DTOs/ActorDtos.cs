using System.ComponentModel.DataAnnotations;
using MovieVault.Api.Entities;

namespace MovieVault.Api.DTOs;

// GET /api/actors liste så man kan finde actorId i Scalar
public class ActorDto
{
    public int ActorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Nationality { get; set; }
}

// Query 5: Actor movie performance
public class ActorPerformanceDto
{
   public int ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;

    // Beregnes ud fra Movies-listen
    public int NumberOfMovies { get; set; }
    public int TotalTicketsSold { get; set; }

    public List<ActorMoviePerformanceDto> Movies { get; set; } = new();
}

// En film, skuespilleren er med i, og hvor mange billetter den har solgt
public class ActorMoviePerformanceDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public RoleType RoleType { get; set; }
    public int TicketsSold { get; set; }
}
