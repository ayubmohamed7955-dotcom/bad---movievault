namespace MovieVault.Api.Entities;

// Et medie (avis, tv-kanal, magasin ...), som anmeldere skriver for.
// Oplysningerne om mediet gemmes ét sted, så de ikke gentages på hver anmelder eller anmeldelse.
public class MediaOutlet
{
    public int MediaOutletId { get; set; }
    public string Name { get; set; } = string.Empty;
    public MediaOutletType OutletType { get; set; }
    public string? Website { get; set; }

    // employs: 1:N - et medie kan have flere anmeldere
    public List<Reviewer> Reviewers { get; set; } = new();

    // publishes: 1:N - et medie kan udgive mange anmeldelser
    public List<Review> Reviews { get; set; } = new();
}
