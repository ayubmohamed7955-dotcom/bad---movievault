namespace MovieVault.Api.Entities;

// Junction-entity for N:N features

public class MovieActor
{
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public int ActorId { get; set; }
    public Actor Actor { get; set; } = null!;
    public string CharacterName { get; set; } = string.Empty;
    public RoleType RoleType { get; set; }
}