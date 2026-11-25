namespace MovieVault.Simulator;

// Hver klasse svarer til en sektion i appsettings.json / miljøvariabler.
// Navnene matcher compose.yaml: MovieVaultApi__BaseUrl, ServiceAccount__UserName osv.

// Sektion "MovieVaultApi"
public class MovieVaultApiOptions
{
    // Lokalt: http://localhost:5019. I Docker: http://api:8080 (service-navnet)
    public string BaseUrl { get; set; } = string.Empty;
}

// Sektion "ServiceAccount" - samme konto som API'et seeder med Admin-rollen
public class ServiceAccountOptions
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Sektion "Simulator"
public class SimulatorOptions
{
    // Sekunder mellem hvert simuleret salg
    public int IntervalSeconds { get; set; } = 15;

    // Et salg får mellem 1 og dette antal billetter
    public int MaxTicketsPerSale { get; set; } = 4;
}
