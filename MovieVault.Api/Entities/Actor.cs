namespace MovieVault.Api.Entities;
using MovieVault.Api.Auth;

public class Actor
{
    public int ActorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string? Nationality { get; set; }
    public string? Gender { get; set; }
    public List<MovieActor> Roles { get; set; } = new();
    public string? UserId { get; set; }
    public ApiUser? User { get; set; }
}