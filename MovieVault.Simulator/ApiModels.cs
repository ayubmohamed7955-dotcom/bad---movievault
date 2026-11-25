namespace MovieVault.Simulator;

// Simulatorens egne DTO'er. Den refererer IKKE til API-projektet:
// den er en selvstændig klient, der kun kender API'et gennem JSON over HTTP.
// Vi tager kun de felter med, vi har brug for. Resten af JSON'en ignoreres.

// POST /api/auth/login
public record LoginRequest(string UserName, string Password);
public record LoginResponse(string Token, int ExpiresInMinutes);

// GET /api/movies
public record MovieDto(int MovieId, string Title);

// GET /api/movies/{movieId}/screenings
public record ScreeningDto(
    int ScreeningId,
    int MovieId,
    string MovieTitle,
    string CinemaName,
    DateTime? StartTime,
    string Status,
    int AvailableSeats);

// POST /api/ticket-sales
public record CreateTicketSaleRequest(int ScreeningId, int TotalTickets);
public record TicketSaleDto(int TicketSaleId, int TotalTickets, decimal TotalPrice);
