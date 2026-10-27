namespace MovieVault.Api.Entities;

public class ProductionCompany
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? FoundingYear { get; set; }
    public string? Country { get; set; }

    // produces: 1:N - én production company kan producere mange film
    public List<Movie> Movies { get; set; } = new();

    // owns: 1:N - én production company kan eje mange studios
    public List<Studio> Studios { get; set; } = new();
}