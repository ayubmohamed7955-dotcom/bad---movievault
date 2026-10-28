namespace MovieVault.Api.Entities;

public class Location
{
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    public LocationType LocationType { get; set; }
    
    // filmed: N:N - én location kan bruges i mange film (via MovieLocation)
    public List<MovieLocation> Movies { get; set; } = new();
}