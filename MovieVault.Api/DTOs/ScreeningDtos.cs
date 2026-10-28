using System.ComponentModel.DataAnnotations;
using MovieVault.Api.Entities;

namespace MovieVault.Api.DTOs;

// Bruges når en ny screening oprettes (POST)
public class CreateScreeningDto
{
    [Range(1, int.MaxValue, ErrorMessage = "MovieId must be a valid id.")]
    public int MovieId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ScreenId must be a valid id.")]
    public int ScreenId { get; set; }

    // StartTime bliver ikke valideret. Datetime kan ikke være null.
    [Required(ErrorMessage = "StartTime is required.")]
    public DateTime? StartTime { get; set; }

    [Range(0, 100000, ErrorMessage = "Ticket price cannot be negative.")]
    public decimal TicketPrice { get; set; }
}

// Bruges når en screening opdateres eller annulleres (PUT)
public class UpdateScreeningDto
{
    [Range(1, int.MaxValue, ErrorMessage = "ScreenId must be a valid id.")]
    public int ScreenId { get; set; }

    [Required(ErrorMessage = "StartTime is required.")]
    public DateTime? StartTime { get; set; }

    [Range(0, 100000, ErrorMessage = "Ticket price cannot be negative.")]
    public decimal TicketPrice { get; set; }

    // "Scheduled" eller "Cancelled"
    // Bruges til at sikre int ikke bliver accepteret, fx "status: 1"
    [EnumDataType(typeof(ScreeningStatus))]
    public ScreeningStatus Status { get; set; }
}

// Det API'et returnerer for en screening
public class ScreeningDto
{
    public int ScreeningId { get; set; }
    public DateTime? StartTime { get; set; }
    public decimal TicketPrice { get; set; }
    public string Status { get; set; } = string.Empty;

    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;

    public int ScreenId { get; set; }
    public int ScreenNumber { get; set; }
    public int SeatingCapacity { get; set; }

    public int CinemaId { get; set; }
    public string CinemaName { get; set; } = string.Empty;

    // Beregnes ud fra ticket sales, gemmes ikke i databasen
    public int TicketsSold { get; set; }
    public int AvailableSeats { get; set; }
}

// Query 3: Upcoming screenings
public class UpcomingScreeningDto
{
    public int ScreeningId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int ScreenNumber { get; set; }
    public DateTime? StartTime { get; set; }
    public decimal TicketPrice { get; set; }
}
