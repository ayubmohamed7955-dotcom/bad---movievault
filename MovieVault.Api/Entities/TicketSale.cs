using System.ComponentModel.DataAnnotations;

namespace MovieVault.Api.Models;

public class TicketSale
{
    public int TicketSaleId { get; set; }

    public DateTime SaleDate { get; set; }

    // Ét salg kan indeholde flere billetter (fx en gruppe der køber sammen)
    [Range(1, int.MaxValue, ErrorMessage = "Total tickets must be at least 1.")]
    public int TotalTickets { get; set; }

    // FK: billetsalg registreres mod en screening
    public int ScreeningId { get; set; }
    public Screening Screening { get; set; } = null!;
}
