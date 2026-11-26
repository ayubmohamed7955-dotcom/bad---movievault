using MovieVault.Api.Entities;

namespace MovieVault.Api.DTOs;

// Det API'et returnerer for et medie
public class MediaOutletDto
{
    public int MediaOutletId { get; set; }
    public string Name { get; set; } = string.Empty;
    public MediaOutletType OutletType { get; set; }
    public string? Website { get; set; }
    public int ReviewCount { get; set; }
}
