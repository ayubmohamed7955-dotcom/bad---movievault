namespace MovieVault.Api.Entities;
using MovieVault.Api.Auth;

public class Director
{
    public int DirectorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string? Nationality { get; set; }

    // directs: 1:N - én director kan instruere mange film
    public List<Movie> Movies { get; set; } = new();
    public string? UserId { get; set; }
    public ApiUser? User { get; set; }
}