using System.ComponentModel.DataAnnotations;
using MovieVault.Api.Entities;

namespace MovieVault.Api.DTOs;

// POST /api/movies/{movieId}/cast

public class AddCastMemberDto
{
    [Range(1, int.MaxValue, ErrorMessage = "ActorId must be a valid id.")]
    public int ActorId { get; set; }

    [Required(ErrorMessage = "Character name is required.")]
    [StringLength(100, ErrorMessage = "Character name can be at most 100 characters.")]
    public string CharacterName { get; set; } = string.Empty;

    [EnumDataType(typeof(RoleType), ErrorMessage = "Role type must be Lead, Supporting or Cameo.")]
    public RoleType RoleType { get; set; }
}

// POST /api/movies/{movieId}/locations
public class AddFilmingLocationDto
{
    [Range(1, int.MaxValue, ErrorMessage = "LocationId must be a valid id.")]
    public int LocationId { get; set; }

    [Range(0, 3650, ErrorMessage = "Shooting days must be between 0 and 3650.")]
    public int? ShootingDays { get; set; }
}

// Responses genbruger CastMemberDto og FilmingLocationDto fra MovieDtos.cs