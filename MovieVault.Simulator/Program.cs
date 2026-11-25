using Microsoft.Extensions.Options;
using MovieVault.Simulator;

var builder = Host.CreateApplicationBuilder(args);

// Konfiguration: appsettings.json -> user secrets (lokalt) -> miljøvariabler (Docker)
builder.Services.Configure<MovieVaultApiOptions>(builder.Configuration.GetSection("MovieVaultApi"));
builder.Services.Configure<ServiceAccountOptions>(builder.Configuration.GetSection("ServiceAccount"));
builder.Services.Configure<SimulatorOptions>(builder.Configuration.GetSection("Simulator"));

// Navngiven HttpClient. IHttpClientFactory styrer de underliggende forbindelser,
// så vi undgår at oprette (og lække) en ny HttpClient for hver request.
builder.Services.AddHttpClient("MovieVaultApi", (sp, client) =>
{
    var api = sp.GetRequiredService<IOptions<MovieVaultApiOptions>>().Value;
    client.BaseAddress = new Uri(api.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Singleton: én klient, der husker sit token mellem runderne
builder.Services.AddSingleton<MovieVaultApiClient>();

// Selve baggrundsservicen
builder.Services.AddHostedService<TicketSaleSimulator>();

var host = builder.Build();
host.Run();
