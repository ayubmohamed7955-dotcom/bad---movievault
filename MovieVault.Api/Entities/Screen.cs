using System.ComponentModel.DataAnnotations;

namespace MovieVault.Api.Models;

public class Screen
{
    public int ScreenId { get; set; }

    // Salens nummer i biografen (fx Sal 1, Sal 2)
    [Range(1, int.MaxValue)]
    public int ScreenNumber { get; set; }

    // Bruges til at tjekke at en screening ikke sælger flere billetter end der er pladser
    [Range(1, int.MaxValue)]
    public int SeatingCapacity { get; set; }

    [MaxLength(50)]
    public string? ScreenType { get; set; } // fx "Standard", "IMAX", "3D"

    // FK: en sal hører til præcis én biograf
    public int CinemaId { get; set; }
    public Cinema Cinema { get; set; } = null!;

    // 1:N - en sal har mange screenings
    public ICollection<Screening> Screenings { get; set; } = new List<Screening>();
}
