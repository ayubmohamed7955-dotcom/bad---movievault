namespace MovieVault.Api.Entities;

// En anmeldelse af en film.
// Forfatterens navn og mediets navn/website gemmes IKKE her - kun fremmednøgler (normalisering).
public class Review
{
    public int ReviewId { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    // 1-5 stjerner (check constraint i DbContext)
    public int Rating { get; set; }

    // Sættes af serveren i UTC, når anmeldelsen oprettes
    public DateTime PublishedAt { get; set; }

    // reviews: N:1 - en film kan have 0..N anmeldelser
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    // writes: N:1 - forfatteren
    public int ReviewerId { get; set; }
    public Reviewer Reviewer { get; set; } = null!;

    // publishes: N:1 - det medie anmeldelsen blev udgivet i.
    // Kopieres fra anmelderen ved oprettelse, så anmeldelsen beholder sit medie,
    // selvom anmelderen senere skifter arbejdsplads.
    public int MediaOutletId { get; set; }
    public MediaOutlet MediaOutlet { get; set; } = null!;
}
