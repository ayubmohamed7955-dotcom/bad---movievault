namespace MovieVault.Api.Entities;

public class Location
{
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    public LocationType LocationType { get; set; }
    
    // has: 1:N - én location kan have mange film
    public List<MovieLocation> Movies { get; set; } = new();
}