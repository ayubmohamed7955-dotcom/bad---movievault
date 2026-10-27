namespace MovieVault.Api.Entities;

public class Studio
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    public int? NumberofSoundStages { get; set; }
    public int? MaxStageCapacity { get; set; }

    // owns: et studio SKAL have en ejer -> ikke nullable FK
    public int ProductionCompanyId { get; set; }
    public ProductionCompany ProductionCompany { get; set; } = null!;
}