using System.ComponentModel.DataAnnotations;

namespace MovieVault.Api.Entities;

public class Cinema
{
    public int CinemaId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string StreetAddress { get; set; } = string.Empty;

    public int? OpeningYear { get; set; }

    // 1:N - en biograf har mange sale
    public ICollection<Screen> Screens { get; set; } = new List<Screen>();
}
