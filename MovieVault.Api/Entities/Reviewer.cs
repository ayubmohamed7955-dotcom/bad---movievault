using MovieVault.Api.Auth;

namespace MovieVault.Api.Entities;

// Anmelderens profil. Forbundet til en Identity-bruger på samme måde som Actor og Director,
// så serveren kan finde anmelderen ud fra JWT'en (klienten kan ikke vælge en anden anmelder).
public class Reviewer
{
    public int ReviewerId { get; set; }
    public string Name { get; set; } = string.Empty;

    // works for: N:1 - anmelderen repræsenterer ét medie
    public int MediaOutletId { get; set; }
    public MediaOutlet MediaOutlet { get; set; } = null!;

    // 1:1 til Identity-brugeren (optional, så seed-data kan oprettes før brugerne)
    public string? UserId { get; set; }
    public ApiUser? User { get; set; }

    // writes: 1:N
    public List<Review> Reviews { get; set; } = new();
}
