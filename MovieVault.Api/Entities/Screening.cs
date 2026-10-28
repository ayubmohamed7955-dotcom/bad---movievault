using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MovieVault.Api.Entities;

public class Screening
{
    public int ScreeningId { get; set; }

    public DateTime StartTime { get; set; }

    [Precision(10, 2)]
    [Range(0, double.MaxValue, ErrorMessage = "Ticket price cannot be negative.")]
    public decimal TicketPrice { get; set; }

    public ScreeningStatus Status { get; set; } = ScreeningStatus.Scheduled;

    // FK: en screening skal referere til en film
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    // FK: en screening skal referere til en sal
    public int ScreenId { get; set; }
    public Screen Screen { get; set; } = null!;

    // 1:N - en screening har mange billetsalg
    public ICollection<TicketSale> TicketSales { get; set; } = new List<TicketSale>();
}
