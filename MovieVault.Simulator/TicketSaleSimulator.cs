using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace MovieVault.Simulator;

// Background service: kører i hele programmets levetid og laver ét simuleret salg pr. runde
public class TicketSaleSimulator : BackgroundService
{
    private readonly MovieVaultApiClient _api;
    private readonly SimulatorOptions _options;
    private readonly ILogger<TicketSaleSimulator> _logger;

    public TicketSaleSimulator(
        MovieVaultApiClient api,
        IOptions<SimulatorOptions> options,
        ILogger<TicketSaleSimulator> logger)
    {
        _api = api;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Ticket sale simulator started. One sale every {Seconds} seconds.",
            _options.IntervalSeconds);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.IntervalSeconds));

        try
        {
            do
            {
                try
                {
                    await SimulateOneSaleAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw; // Programmet lukker ned - stop loop
                }
                catch (HttpRequestException ex)
                {
                    // API'et svarer ikke, eller svarede 5xx. Fx under opstart i Docker.
                    _logger.LogWarning("API request failed: {Message}. Trying again next round.", ex.Message);
                }
                catch (TaskCanceledException)
                {
                    // HttpClient-timeout (10 sek.)
                    _logger.LogWarning("API request timed out. Trying again next round.");
                }
                catch (Exception ex)
                {
                    // Alt andet. Uden denne catch ville én fejl stoppe hele servicen.
                    _logger.LogError(ex, "Unexpected error. Trying again next round.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Normal nedlukning
        }

        _logger.LogInformation("Ticket sale simulator stopped.");
    }

    private async Task SimulateOneSaleAsync(CancellationToken ct)
    {
        // 1. Hent film
        var movies = await _api.GetMoviesAsync(ct);
        if (movies.Count == 0)
        {
            _logger.LogInformation("No movies found. Skipping this round.");
            return;
        }

        // 2. Vælg en tilfældig film, så salget fordeles over forskellige film
        var movie = movies[Random.Shared.Next(movies.Count)];

        // 3. Hent filmens screenings og behold dem, der kan sælges billetter til
        var screenings = await _api.GetScreeningsForMovieAsync(movie.MovieId, ct);
        var open = screenings
            .Where(s => s.Status == "Scheduled"
                        && s.StartTime > DateTime.Now
                        && s.AvailableSeats > 0)
            .ToList();

        if (open.Count == 0)
        {
            _logger.LogInformation("No open screenings for '{Title}'. Skipping this round.", movie.Title);
            return;
        }

        var screening = open[Random.Shared.Next(open.Count)];

        // 4. Tilfældigt antal billetter. Vi begrænser det IKKE til de ledige pladser:
        //    API'et er den, der håndhæver kapaciteten, ligesom ved et manuelt salg.
        var tickets = Random.Shared.Next(1, _options.MaxTicketsPerSale + 1);

        // 5. Opret salget gennem API'et
        using var response = await _api.CreateTicketSaleAsync(screening.ScreeningId, tickets, ct);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var sale = await response.Content.ReadFromJsonAsync<TicketSaleDto>(ct);
            _logger.LogInformation(
                "Sale {SaleId}: {Tickets} ticket(s) for '{Title}' at {Cinema}, screening {ScreeningId} ({Start}). Total {Price} kr.",
                sale?.TicketSaleId, tickets, movie.Title, screening.CinemaName,
                screening.ScreeningId, screening.StartTime, sale?.TotalPrice);
        }
        else if ((int)response.StatusCode is >= 400 and < 500)
        {
            // Afvist af en forretningsregel (fx kapacitet) eller adgangskontrol. Ikke en fejl i simulatoren.
            var problem = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning(
                "Sale of {Tickets} ticket(s) for screening {ScreeningId} rejected with {Status}: {Problem}",
                tickets, screening.ScreeningId, (int)response.StatusCode, problem);
        }
        else
        {
            // 5xx: kaster HttpRequestException, som fanges i ExecuteAsync
            response.EnsureSuccessStatusCode();
        }
    }
}
