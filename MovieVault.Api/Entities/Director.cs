namespace MovieVault.Api.Entities;

public class Director
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string? Nationality { get; set; }

    // directs: 1:N - én director kan instruere mange film
    public List<Movie> Movies { get; set; } = new();
}