using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Models;

namespace MovieVault.Api.Data.Seed;

// Seed data for biograf-siden (person B).
// Kaldes fra DbContext.OnModelCreating med: modelBuilder.SeedCinemaData();
// Screenings bruger MovieId 1-5, så Movie-seed skal have de id'er.
public static class CinemaSeedData
{
    public static void SeedCinemaData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cinema>().HasData(Cinemas);
        modelBuilder.Entity<Screen>().HasData(Screens);
        modelBuilder.Entity<Screening>().HasData(Screenings);
        modelBuilder.Entity<TicketSale>().HasData(TicketSales);
    }

    public static readonly Cinema[] Cinemas =
    {
        new Cinema { CinemaId = 1, Name = "Nordlys Bio", City = "Aarhus", StreetAddress = "Banegårdsgade 12", OpeningYear = 1998 },
        new Cinema { CinemaId = 2, Name = "Kronborg Kino", City = "København", StreetAddress = "Vesterbrogade 45", OpeningYear = 2005 },
        new Cinema { CinemaId = 3, Name = "Fyrtårnet Biograf", City = "Odense", StreetAddress = "Kongensgade 8", OpeningYear = 2012 },
    };

    public static readonly Screen[] Screens =
    {
        new Screen { ScreenId = 1, CinemaId = 1, ScreenNumber = 1, SeatingCapacity = 120, ScreenType = "Standard" },
        new Screen { ScreenId = 2, CinemaId = 1, ScreenNumber = 2, SeatingCapacity = 80,  ScreenType = "3D" },
        new Screen { ScreenId = 3, CinemaId = 1, ScreenNumber = 3, SeatingCapacity = 40,  ScreenType = "Standard" },
        new Screen { ScreenId = 4, CinemaId = 2, ScreenNumber = 1, SeatingCapacity = 200, ScreenType = "IMAX" },
        new Screen { ScreenId = 5, CinemaId = 2, ScreenNumber = 2, SeatingCapacity = 100, ScreenType = "Standard" },
        new Screen { ScreenId = 6, CinemaId = 3, ScreenNumber = 1, SeatingCapacity = 150, ScreenType = "Standard" },
        new Screen { ScreenId = 7, CinemaId = 3, ScreenNumber = 2, SeatingCapacity = 60,  ScreenType = "3D" },
    };

    // Blanding af afholdte og kommende screenings, så "upcoming" kan testes.
    // Screening 6 (sal 3, 40 pladser) har 38 solgte billetter - god til at demonstrere kapacitetstjekket.
    // Screening 12 er annulleret.
    public static readonly Screening[] Screenings =
    {
        new Screening { ScreeningId = 1,  MovieId = 1, ScreenId = 1, StartTime = new DateTime(2026, 10, 10, 19, 0, 0),  TicketPrice = 110m, Status = ScreeningStatus.Scheduled },
        new Screening { ScreeningId = 2,  MovieId = 1, ScreenId = 4, StartTime = new DateTime(2026, 12, 5, 20, 0, 0),   TicketPrice = 150m, Status = ScreeningStatus.Scheduled },
        new Screening { ScreeningId = 3,  MovieId = 2, ScreenId = 2, StartTime = new DateTime(2026, 10, 15, 18, 30, 0), TicketPrice = 120m, Status = ScreeningStatus.Scheduled },
        new Screening { ScreeningId = 4,  MovieId = 2, ScreenId = 6, StartTime = new DateTime(2026, 12, 12, 19, 30, 0), TicketPrice = 100m, Status = ScreeningStatus.Scheduled },
        new Screening { ScreeningId = 5,  MovieId = 3, ScreenId = 5, StartTime = new DateTime(2026, 11, 1, 17, 0, 0),   TicketPrice = 95m,  Status = ScreeningStatus.Scheduled },
        new Screening { ScreeningId = 6,  MovieId = 3, ScreenId = 3, StartTime = new DateTime(2026, 12, 20, 21, 0, 0),  TicketPrice = 90m,  Status = ScreeningStatus.Scheduled },
        new Screening { ScreeningId = 7,  MovieId = 4, ScreenId = 4, StartTime = new DateTime(2026, 11, 20, 20, 0, 0),  TicketPrice = 160m, Status = ScreeningStatus.Scheduled },
        new Screening { ScreeningId = 8,  MovieId = 4, ScreenId = 7, StartTime = new DateTime(2027, 1, 8, 19, 0, 0),    TicketPrice = 115m, Status = ScreeningStatus.Scheduled },
        new Screening { ScreeningId = 9,  MovieId = 5, ScreenId = 6, StartTime = new DateTime(2026, 10, 25, 16, 0, 0),  TicketPrice = 85m,  Status = ScreeningStatus.Scheduled },
        new Screening { ScreeningId = 10, MovieId = 5, ScreenId = 1, StartTime = new DateTime(2027, 1, 15, 18, 0, 0),   TicketPrice = 105m, Status = ScreeningStatus.Scheduled },
        new Screening { ScreeningId = 11, MovieId = 1, ScreenId = 7, StartTime = new DateTime(2027, 1, 22, 20, 30, 0),  TicketPrice = 115m, Status = ScreeningStatus.Scheduled },
        new Screening { ScreeningId = 12, MovieId = 2, ScreenId = 5, StartTime = new DateTime(2026, 11, 28, 19, 0, 0),  TicketPrice = 100m, Status = ScreeningStatus.Cancelled },
    };

    public static readonly TicketSale[] TicketSales =
    {
        new TicketSale { TicketSaleId = 1,  ScreeningId = 1,  SaleDate = new DateTime(2026, 10, 1, 10, 15, 0),  TotalTickets = 4 },
        new TicketSale { TicketSaleId = 2,  ScreeningId = 1,  SaleDate = new DateTime(2026, 10, 5, 14, 30, 0),  TotalTickets = 2 },
        new TicketSale { TicketSaleId = 3,  ScreeningId = 1,  SaleDate = new DateTime(2026, 10, 10, 18, 5, 0),  TotalTickets = 6 },
        new TicketSale { TicketSaleId = 4,  ScreeningId = 2,  SaleDate = new DateTime(2026, 10, 20, 9, 45, 0),  TotalTickets = 3 },
        new TicketSale { TicketSaleId = 5,  ScreeningId = 2,  SaleDate = new DateTime(2026, 10, 24, 20, 10, 0), TotalTickets = 5 },
        new TicketSale { TicketSaleId = 6,  ScreeningId = 3,  SaleDate = new DateTime(2026, 10, 8, 12, 0, 0),   TotalTickets = 2 },
        new TicketSale { TicketSaleId = 7,  ScreeningId = 3,  SaleDate = new DateTime(2026, 10, 14, 16, 20, 0), TotalTickets = 4 },
        new TicketSale { TicketSaleId = 8,  ScreeningId = 4,  SaleDate = new DateTime(2026, 10, 22, 11, 30, 0), TotalTickets = 3 },
        new TicketSale { TicketSaleId = 9,  ScreeningId = 5,  SaleDate = new DateTime(2026, 10, 18, 13, 0, 0),  TotalTickets = 1 },
        new TicketSale { TicketSaleId = 10, ScreeningId = 5,  SaleDate = new DateTime(2026, 10, 26, 17, 45, 0), TotalTickets = 6 },
        new TicketSale { TicketSaleId = 11, ScreeningId = 6,  SaleDate = new DateTime(2026, 10, 19, 10, 0, 0),  TotalTickets = 20 },
        new TicketSale { TicketSaleId = 12, ScreeningId = 6,  SaleDate = new DateTime(2026, 10, 23, 15, 30, 0), TotalTickets = 18 },
        new TicketSale { TicketSaleId = 13, ScreeningId = 7,  SaleDate = new DateTime(2026, 10, 12, 19, 0, 0),  TotalTickets = 8 },
        new TicketSale { TicketSaleId = 14, ScreeningId = 7,  SaleDate = new DateTime(2026, 10, 21, 8, 50, 0),  TotalTickets = 2 },
        new TicketSale { TicketSaleId = 15, ScreeningId = 8,  SaleDate = new DateTime(2026, 10, 25, 21, 15, 0), TotalTickets = 4 },
        new TicketSale { TicketSaleId = 16, ScreeningId = 9,  SaleDate = new DateTime(2026, 10, 20, 12, 40, 0), TotalTickets = 5 },
        new TicketSale { TicketSaleId = 17, ScreeningId = 9,  SaleDate = new DateTime(2026, 10, 25, 15, 10, 0), TotalTickets = 3 },
        new TicketSale { TicketSaleId = 18, ScreeningId = 10, SaleDate = new DateTime(2026, 10, 26, 10, 30, 0), TotalTickets = 2 },
        new TicketSale { TicketSaleId = 19, ScreeningId = 11, SaleDate = new DateTime(2026, 10, 26, 18, 0, 0),  TotalTickets = 7 },
    };
}
