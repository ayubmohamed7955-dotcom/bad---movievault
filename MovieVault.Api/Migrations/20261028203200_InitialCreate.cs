using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MovieVault.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Actors",
                columns: table => new
                {
                    ActorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actors", x => x.ActorId);
                });

            migrationBuilder.CreateTable(
                name: "Cinemas",
                columns: table => new
                {
                    CinemaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StreetAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OpeningYear = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cinemas", x => x.CinemaId);
                });

            migrationBuilder.CreateTable(
                name: "Directors",
                columns: table => new
                {
                    DirectorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directors", x => x.DirectorId);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    LocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    LocationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.LocationId);
                });

            migrationBuilder.CreateTable(
                name: "ProductionCompanies",
                columns: table => new
                {
                    ProductionCompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FoundingYear = table.Column<int>(type: "int", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionCompanies", x => x.ProductionCompanyId);
                });

            migrationBuilder.CreateTable(
                name: "Screens",
                columns: table => new
                {
                    ScreenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScreenNumber = table.Column<int>(type: "int", nullable: false),
                    SeatingCapacity = table.Column<int>(type: "int", nullable: false),
                    ScreenType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CinemaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Screens", x => x.ScreenId);
                    table.CheckConstraint("CK_Screen_SeatingCapacity", "[SeatingCapacity] > 0");
                    table.ForeignKey(
                        name: "FK_Screens_Cinemas_CinemaId",
                        column: x => x.CinemaId,
                        principalTable: "Cinemas",
                        principalColumn: "CinemaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    MovieId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RuntimeMinutes = table.Column<int>(type: "int", nullable: true),
                    AgeRating = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DirectorId = table.Column<int>(type: "int", nullable: true),
                    ProductionCompanyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.MovieId);
                    table.CheckConstraint("CK_Movie_RuntimeMinutes", "[RuntimeMinutes] IS NULL OR [RuntimeMinutes] > 0");
                    table.ForeignKey(
                        name: "FK_Movies_Directors_DirectorId",
                        column: x => x.DirectorId,
                        principalTable: "Directors",
                        principalColumn: "DirectorId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Movies_ProductionCompanies_ProductionCompanyId",
                        column: x => x.ProductionCompanyId,
                        principalTable: "ProductionCompanies",
                        principalColumn: "ProductionCompanyId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Studios",
                columns: table => new
                {
                    StudioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    NumberOfSoundStages = table.Column<int>(type: "int", nullable: true),
                    MaxStageCapacity = table.Column<int>(type: "int", nullable: true),
                    ProductionCompanyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Studios", x => x.StudioId);
                    table.CheckConstraint("CK_Studio_MaxStageCapacity", "[MaxStageCapacity] IS NULL OR [MaxStageCapacity] > 0");
                    table.CheckConstraint("CK_Studio_NumberOfSoundStages", "[NumberOfSoundStages] IS NULL OR [NumberOfSoundStages] >= 0");
                    table.ForeignKey(
                        name: "FK_Studios_ProductionCompanies_ProductionCompanyId",
                        column: x => x.ProductionCompanyId,
                        principalTable: "ProductionCompanies",
                        principalColumn: "ProductionCompanyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovieActors",
                columns: table => new
                {
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    ActorId = table.Column<int>(type: "int", nullable: false),
                    CharacterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieActors", x => new { x.MovieId, x.ActorId });
                    table.ForeignKey(
                        name: "FK_MovieActors_Actors_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Actors",
                        principalColumn: "ActorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieActors_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "MovieId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieLocations",
                columns: table => new
                {
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ShootingDays = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieLocations", x => new { x.MovieId, x.LocationId });
                    table.CheckConstraint("CK_MovieLocation_ShootingDays", "[ShootingDays] IS NULL OR [ShootingDays] > 0");
                    table.ForeignKey(
                        name: "FK_MovieLocations_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieLocations_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "MovieId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionRecords",
                columns: table => new
                {
                    ProductionRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Budget = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: true),
                    ShootingStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ShootingEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    MovieId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionRecords", x => x.ProductionRecordId);
                    table.CheckConstraint("CK_ProductionRecord_Budget", "[Budget] IS NULL OR [Budget] >= 0");
                    table.CheckConstraint("CK_ProductionRecord_ShootingDates", "[ShootingStartDate] IS NULL OR [ShootingEndDate] IS NULL OR [ShootingEndDate] >= [ShootingStartDate]");
                    table.ForeignKey(
                        name: "FK_ProductionRecords_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "MovieId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Screenings",
                columns: table => new
                {
                    ScreeningId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TicketPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    ScreenId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Screenings", x => x.ScreeningId);
                    table.CheckConstraint("CK_Screening_TicketPrice", "[TicketPrice] >= 0");
                    table.ForeignKey(
                        name: "FK_Screenings_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "MovieId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Screenings_Screens_ScreenId",
                        column: x => x.ScreenId,
                        principalTable: "Screens",
                        principalColumn: "ScreenId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketSales",
                columns: table => new
                {
                    TicketSaleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalTickets = table.Column<int>(type: "int", nullable: false),
                    ScreeningId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketSales", x => x.TicketSaleId);
                    table.CheckConstraint("CK_TicketSale_TotalTickets", "[TotalTickets] > 0");
                    table.ForeignKey(
                        name: "FK_TicketSales_Screenings_ScreeningId",
                        column: x => x.ScreeningId,
                        principalTable: "Screenings",
                        principalColumn: "ScreeningId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Actors",
                columns: new[] { "ActorId", "BirthDate", "Gender", "Name", "Nationality" },
                values: new object[,]
                {
                    { 1, new DateOnly(1980, 2, 17), "Male", "Mikkel Strand", "Danish" },
                    { 2, new DateOnly(1988, 6, 30), "Female", "Clara Holt", "Swedish" },
                    { 3, new DateOnly(1955, 9, 8), "Male", "Oskar Brandt", "German" },
                    { 4, new DateOnly(1992, 1, 12), "Female", "Isabela Duarte", "Brazilian" },
                    { 5, new DateOnly(1985, 4, 25), "Male", "Marcus Reed", "British" },
                    { 6, new DateOnly(1990, 10, 3), "Female", "Freja Nygaard", "Danish" },
                    { 7, new DateOnly(1995, 12, 19), "Female", "Elena Petrova", "Bulgarian" },
                    { 8, new DateOnly(1983, 8, 14), "Male", "Daniel Okafor", "Nigerian" },
                    { 9, new DateOnly(1987, 3, 6), "Female", "Grace Whitfield", "American" },
                    { 10, new DateOnly(1993, 7, 21), "Female", "Yuki Tanabe", "Japanese" }
                });

            migrationBuilder.InsertData(
                table: "Cinemas",
                columns: new[] { "CinemaId", "City", "Name", "OpeningYear", "StreetAddress" },
                values: new object[,]
                {
                    { 1, "Aarhus", "Nordlys Bio", 1998, "Banegårdsgade 12" },
                    { 2, "København", "Kronborg Kino", 2005, "Vesterbrogade 45" },
                    { 3, "Odense", "Fyrtårnet Biograf", 2012, "Kongensgade 8" }
                });

            migrationBuilder.InsertData(
                table: "Directors",
                columns: new[] { "DirectorId", "BirthDate", "Name", "Nationality" },
                values: new object[,]
                {
                    { 1, new DateOnly(1975, 3, 14), "Ida Mortensen", "Danish" },
                    { 2, new DateOnly(1982, 7, 2), "Lucas Ferreira", "Brazilian" },
                    { 3, new DateOnly(1979, 11, 23), "Hannah Weiss", "German" },
                    { 4, new DateOnly(1968, 5, 9), "Kenji Arata", "Japanese" }
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "LocationId", "City", "Country", "LocationType", "Name" },
                values: new object[,]
                {
                    { 1, "Hanstholm", "Denmark", "Building", "Hanstholm Lighthouse" },
                    { 2, "Esbjerg", "Denmark", "Street", "Old Harbor Quay" },
                    { 3, "Manchester", "United Kingdom", "Set", "Neon District Set" },
                    { 4, "Lofoten", "Norway", "Landscape", "Lofoten Coastline" },
                    { 5, "Kingman", "USA", "Street", "Desert Highway Stretch" }
                });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "MovieId", "AgeRating", "DirectorId", "ProductionCompanyId", "ReleaseDate", "RuntimeMinutes", "Status", "Title" },
                values: new object[] { 6, null, null, null, null, null, "Planned", "Winter Protocol" });

            migrationBuilder.InsertData(
                table: "ProductionCompanies",
                columns: new[] { "ProductionCompanyId", "Country", "FoundingYear", "Name" },
                values: new object[,]
                {
                    { 1, "Denmark", 1994, "Nordic Frame Pictures" },
                    { 2, "United Kingdom", 2001, "Silverline Studios Group" },
                    { 3, "USA", 1988, "Meridian Films" }
                });

            migrationBuilder.InsertData(
                table: "MovieActors",
                columns: new[] { "ActorId", "MovieId", "CharacterName", "RoleType" },
                values: new object[] { 8, 6, "Agent Marsh", "Lead" });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "MovieId", "AgeRating", "DirectorId", "ProductionCompanyId", "ReleaseDate", "RuntimeMinutes", "Status", "Title" },
                values: new object[,]
                {
                    { 1, "15", 1, 1, new DateOnly(2026, 3, 12), 118, "Released", "The Last Lighthouse" },
                    { 2, "11", 2, 2, new DateOnly(2026, 5, 22), 104, "Released", "Echoes of Neon" },
                    { 3, "15", 3, 1, new DateOnly(2026, 8, 7), 126, "Released", "Glass Harbor" },
                    { 4, "7", 4, 3, new DateOnly(2026, 9, 18), 98, "Released", "Northbound" },
                    { 5, "A", 1, 3, new DateOnly(2026, 10, 2), 111, "Released", "Paper Satellites" }
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "ScreenId", "CinemaId", "ScreenNumber", "ScreenType", "SeatingCapacity" },
                values: new object[,]
                {
                    { 1, 1, 1, "Standard", 120 },
                    { 2, 1, 2, "3D", 80 },
                    { 3, 1, 3, "Standard", 40 },
                    { 4, 2, 1, "IMAX", 200 },
                    { 5, 2, 2, "Standard", 100 },
                    { 6, 3, 1, "Standard", 150 },
                    { 7, 3, 2, "3D", 60 }
                });

            migrationBuilder.InsertData(
                table: "Studios",
                columns: new[] { "StudioId", "City", "Country", "MaxStageCapacity", "Name", "NumberOfSoundStages", "ProductionCompanyId" },
                values: new object[,]
                {
                    { 1, "Copenhagen", "Denmark", 300, "Nordic Frame Stages", 4, 1 },
                    { 2, "Manchester", "United Kingdom", 450, "Silverline North Lot", 6, 2 },
                    { 3, "Atlanta", "USA", 800, "Meridian Sound City", 10, 3 },
                    { 4, "Vancouver", "Canada", 400, "Meridian West", 5, 3 }
                });

            migrationBuilder.InsertData(
                table: "MovieActors",
                columns: new[] { "ActorId", "MovieId", "CharacterName", "RoleType" },
                values: new object[,]
                {
                    { 1, 1, "Captain Erik Holm", "Lead" },
                    { 2, 1, "Sofie Lund", "Supporting" },
                    { 3, 1, "The Keeper", "Cameo" },
                    { 2, 2, "Dr. Anna Berg", "Supporting" },
                    { 4, 2, "Maya Reyes", "Lead" },
                    { 5, 2, "Detective Cole", "Supporting" },
                    { 1, 3, "Jonas Vik", "Lead" },
                    { 6, 3, "Harbor Master Ane", "Supporting" },
                    { 7, 3, "Lena", "Supporting" },
                    { 5, 4, "Gas Station Clerk", "Cameo" },
                    { 8, 4, "Tom Ashby", "Lead" },
                    { 9, 4, "Nora Ashby", "Lead" },
                    { 1, 5, "Professor Lind", "Supporting" },
                    { 4, 5, "Mission Control", "Cameo" },
                    { 10, 5, "Ava Chen", "Lead" }
                });

            migrationBuilder.InsertData(
                table: "MovieLocations",
                columns: new[] { "LocationId", "MovieId", "ShootingDays" },
                values: new object[,]
                {
                    { 1, 1, 12 },
                    { 4, 1, 8 },
                    { 3, 2, 30 },
                    { 2, 3, 15 },
                    { 4, 3, 6 },
                    { 5, 4, 20 },
                    { 3, 5, 10 }
                });

            migrationBuilder.InsertData(
                table: "ProductionRecords",
                columns: new[] { "ProductionRecordId", "Budget", "MovieId", "ShootingEndDate", "ShootingStartDate" },
                values: new object[,]
                {
                    { 1, 42000000m, 1, new DateOnly(2025, 1, 31), new DateOnly(2024, 9, 2) },
                    { 2, 65500000m, 2, new DateOnly(2025, 4, 18), new DateOnly(2024, 11, 4) },
                    { 3, 28750000m, 3, new DateOnly(2025, 6, 27), new DateOnly(2025, 2, 10) },
                    { 4, 18000000m, 4, new DateOnly(2025, 7, 15), new DateOnly(2025, 4, 1) },
                    { 5, 90000000m, 5, new DateOnly(2025, 9, 12), new DateOnly(2025, 3, 3) }
                });

            migrationBuilder.InsertData(
                table: "Screenings",
                columns: new[] { "ScreeningId", "MovieId", "ScreenId", "StartTime", "Status", "TicketPrice" },
                values: new object[,]
                {
                    { 1, 1, 1, new DateTime(2026, 10, 10, 19, 0, 0, 0, DateTimeKind.Unspecified), "Scheduled", 110m },
                    { 2, 1, 4, new DateTime(2026, 12, 5, 20, 0, 0, 0, DateTimeKind.Unspecified), "Scheduled", 150m },
                    { 3, 2, 2, new DateTime(2026, 10, 15, 18, 30, 0, 0, DateTimeKind.Unspecified), "Scheduled", 120m },
                    { 4, 2, 6, new DateTime(2026, 12, 12, 19, 30, 0, 0, DateTimeKind.Unspecified), "Scheduled", 100m },
                    { 5, 3, 5, new DateTime(2026, 11, 1, 17, 0, 0, 0, DateTimeKind.Unspecified), "Scheduled", 95m },
                    { 6, 3, 3, new DateTime(2026, 12, 20, 21, 0, 0, 0, DateTimeKind.Unspecified), "Scheduled", 90m },
                    { 7, 4, 4, new DateTime(2026, 11, 20, 20, 0, 0, 0, DateTimeKind.Unspecified), "Scheduled", 160m },
                    { 8, 4, 7, new DateTime(2027, 1, 8, 19, 0, 0, 0, DateTimeKind.Unspecified), "Scheduled", 115m },
                    { 9, 5, 6, new DateTime(2026, 10, 25, 16, 0, 0, 0, DateTimeKind.Unspecified), "Scheduled", 85m },
                    { 10, 5, 1, new DateTime(2027, 1, 15, 18, 0, 0, 0, DateTimeKind.Unspecified), "Scheduled", 105m },
                    { 11, 1, 7, new DateTime(2027, 1, 22, 20, 30, 0, 0, DateTimeKind.Unspecified), "Scheduled", 115m },
                    { 12, 2, 5, new DateTime(2026, 11, 28, 19, 0, 0, 0, DateTimeKind.Unspecified), "Cancelled", 100m }
                });

            migrationBuilder.InsertData(
                table: "TicketSales",
                columns: new[] { "TicketSaleId", "SaleDate", "ScreeningId", "TotalTickets" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 1, 10, 15, 0, 0, DateTimeKind.Unspecified), 1, 4 },
                    { 2, new DateTime(2026, 10, 5, 14, 30, 0, 0, DateTimeKind.Unspecified), 1, 2 },
                    { 3, new DateTime(2026, 10, 10, 18, 5, 0, 0, DateTimeKind.Unspecified), 1, 6 },
                    { 4, new DateTime(2026, 10, 20, 9, 45, 0, 0, DateTimeKind.Unspecified), 2, 3 },
                    { 5, new DateTime(2026, 10, 24, 20, 10, 0, 0, DateTimeKind.Unspecified), 2, 5 },
                    { 6, new DateTime(2026, 10, 8, 12, 0, 0, 0, DateTimeKind.Unspecified), 3, 2 },
                    { 7, new DateTime(2026, 10, 14, 16, 20, 0, 0, DateTimeKind.Unspecified), 3, 4 },
                    { 8, new DateTime(2026, 10, 22, 11, 30, 0, 0, DateTimeKind.Unspecified), 4, 3 },
                    { 9, new DateTime(2026, 10, 18, 13, 0, 0, 0, DateTimeKind.Unspecified), 5, 1 },
                    { 10, new DateTime(2026, 10, 26, 17, 45, 0, 0, DateTimeKind.Unspecified), 5, 6 },
                    { 11, new DateTime(2026, 10, 19, 10, 0, 0, 0, DateTimeKind.Unspecified), 6, 20 },
                    { 12, new DateTime(2026, 10, 23, 15, 30, 0, 0, DateTimeKind.Unspecified), 6, 18 },
                    { 13, new DateTime(2026, 10, 12, 19, 0, 0, 0, DateTimeKind.Unspecified), 7, 8 },
                    { 14, new DateTime(2026, 10, 21, 8, 50, 0, 0, DateTimeKind.Unspecified), 7, 2 },
                    { 15, new DateTime(2026, 10, 25, 21, 15, 0, 0, DateTimeKind.Unspecified), 8, 4 },
                    { 16, new DateTime(2026, 10, 20, 12, 40, 0, 0, DateTimeKind.Unspecified), 9, 5 },
                    { 17, new DateTime(2026, 10, 25, 15, 10, 0, 0, DateTimeKind.Unspecified), 9, 3 },
                    { 18, new DateTime(2026, 10, 26, 10, 30, 0, 0, DateTimeKind.Unspecified), 10, 2 },
                    { 19, new DateTime(2026, 10, 26, 18, 0, 0, 0, DateTimeKind.Unspecified), 11, 7 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovieActors_ActorId",
                table: "MovieActors",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieLocations_LocationId",
                table: "MovieLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_DirectorId",
                table: "Movies",
                column: "DirectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_ProductionCompanyId",
                table: "Movies",
                column: "ProductionCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionRecords_MovieId",
                table: "ProductionRecords",
                column: "MovieId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Screenings_MovieId",
                table: "Screenings",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Screenings_ScreenId",
                table: "Screenings",
                column: "ScreenId");

            migrationBuilder.CreateIndex(
                name: "IX_Screenings_StartTime",
                table: "Screenings",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Screens_CinemaId_ScreenNumber",
                table: "Screens",
                columns: new[] { "CinemaId", "ScreenNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Studios_ProductionCompanyId",
                table: "Studios",
                column: "ProductionCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSales_ScreeningId",
                table: "TicketSales",
                column: "ScreeningId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovieActors");

            migrationBuilder.DropTable(
                name: "MovieLocations");

            migrationBuilder.DropTable(
                name: "ProductionRecords");

            migrationBuilder.DropTable(
                name: "Studios");

            migrationBuilder.DropTable(
                name: "TicketSales");

            migrationBuilder.DropTable(
                name: "Actors");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Screenings");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Screens");

            migrationBuilder.DropTable(
                name: "Directors");

            migrationBuilder.DropTable(
                name: "ProductionCompanies");

            migrationBuilder.DropTable(
                name: "Cinemas");
        }
    }
}
