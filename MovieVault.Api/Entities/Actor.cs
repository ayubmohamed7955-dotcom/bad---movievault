namespace MovieVault.Api.Entities;

public class Actor
{
    public int ActorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string? Nationality { get; set; }
    public string? Gender { get; set; }
    public List<MovieActor> Role { get; set; } = new();
}