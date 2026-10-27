namespace MovieVault.Api.Entities;

public class ProductionRecord
{
    public int ProductionRecordId { get; set; }
    public decimal? Budget { get; set; }
    public DateOnly? ShootingStartDate { get; set; }
    public DateOnly? ShootingEndDate { get; set; }

    // has: 1:1 - én record SKAL høre til en film
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
}