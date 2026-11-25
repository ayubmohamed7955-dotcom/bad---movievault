using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace MovieVault.Simulator;

// Al kommunikation med MovieVault API'et går gennem denne klasse.
// Den logger ind, husker JWT'en og sætter den på hver request.
public class MovieVaultApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ServiceAccountOptions _account;
    private readonly ILogger<MovieVaultApiClient> _logger;

    // Tokenet genbruges, indtil det næsten er udløbet
    private string? _token;
    private DateTime _tokenExpiresUtc = DateTime.MinValue;

    public MovieVaultApiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<ServiceAccountOptions> account,
        ILogger<MovieVaultApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _account = account.Value;
        _logger = logger;
    }

    public async Task<List<MovieDto>> GetMoviesAsync(CancellationToken ct)
    {
        using var response = await SendAsync(HttpMethod.Get, "api/movies", null, ct);
        response.EnsureSuccessStatusCode(); // 5xx -> HttpRequestException
        return await response.Content.ReadFromJsonAsync<List<MovieDto>>(ct) ?? new();
    }

    public async Task<List<ScreeningDto>> GetScreeningsForMovieAsync(int movieId, CancellationToken ct)
    {
        using var response = await SendAsync(HttpMethod.Get, $"api/movies/{movieId}/screenings", null, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ScreeningDto>>(ct) ?? new();
    }

    // Returnerer hele responsen, så simulatoren selv kan se,
    // om salget blev oprettet (201) eller afvist af en forretningsregel (400)
    public Task<HttpResponseMessage> CreateTicketSaleAsync(int screeningId, int totalTickets, CancellationToken ct)
    {
        var body = new CreateTicketSaleRequest(screeningId, totalTickets);
        return SendAsync(HttpMethod.Post, "api/ticket-sales", body, ct);
    }

    // Sender en request. Svarer API'et 401, er tokenet ugyldigt:
    // log ind igen og prøv præcis én gang til.
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body, CancellationToken ct)
    {
        var response = await SendWithTokenAsync(method, url, body, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogInformation("API answered 401 Unauthorized. Logging in again.");
            response.Dispose();
            _token = null;
            response = await SendWithTokenAsync(method, url, body, ct);
        }

        return response;
    }

    private async Task<HttpResponseMessage> SendWithTokenAsync(HttpMethod method, string url, object? body, CancellationToken ct)
    {
        await EnsureLoggedInAsync(ct);

        var client = _httpClientFactory.CreateClient("MovieVaultApi");
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await client.SendAsync(request, ct);
    }

    private async Task EnsureLoggedInAsync(CancellationToken ct)
    {
        // Gyldigt token? Så genbruger vi det
        if (_token is not null && DateTime.UtcNow < _tokenExpiresUtc)
        {
            return;
        }

        var client = _httpClientFactory.CreateClient("MovieVaultApi");
        var login = new LoginRequest(_account.UserName, _account.Password);

        // Login-requesten sendes uden token - endpointet er [AllowAnonymous]
        using var response = await client.PostAsJsonAsync("api/auth/login", login, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Forkert brugernavn/password. Vi logger aldrig selve passwordet.
            throw new InvalidOperationException(
                $"Login failed for service account '{_account.UserName}'. Check ServiceAccount configuration.");
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(ct)
            ?? throw new InvalidOperationException("Login response was empty.");

        _token = result.Token;
        // Forny et minut før udløb, så tokenet ikke udløber midt i en request
        _tokenExpiresUtc = DateTime.UtcNow.AddMinutes(result.ExpiresInMinutes - 1);

        _logger.LogInformation("Logged in as {UserName}. Token valid for {Minutes} minutes.",
            _account.UserName, result.ExpiresInMinutes);
    }
}
