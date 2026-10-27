using System.ComponentModel.DataAnnotations;

namespace MovieVault.Api.DTOs;

// Bruges når et billetsalg registreres (POST)
public class CreateTicketSaleDto
{
    [Range(1, int.MaxValue, ErrorMessage = "ScreeningId must be a valid id.")]
    public int ScreeningId { get; set; }

    // Ét salg kan indeholde flere billetter, men aldrig 0 eller negativt
    [Range(1, int.MaxValue, ErrorMessage = "Total tickets must be at least 1.")]
    public int TotalTickets { get; set; }
}

// Bruges når et billetsalg ændres (PUT)
public class UpdateTicketSaleDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Total tickets must be at least 1.")]
    public int TotalTickets { get; set; }
}

// Det API'et returnerer for et billetsalg
public class TicketSaleDto
{
    public int TicketSaleId { get; set; }
    public DateTime SaleDate { get; set; }
    public int TotalTickets { get; set; }

    public int ScreeningId { get; set; }
    public DateTime ScreeningStartTime { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;

    public decimal TicketPrice { get; set; }
    // TotalTickets * TicketPrice, beregnet
    public decimal TotalPrice { get; set; }
}

// Query 4: Ticket sales by movie
public class MovieTicketSalesDto
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int TotalTicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}
