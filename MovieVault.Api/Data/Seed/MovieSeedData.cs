using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Entities;

namespace MovieVault.Api.Data.Seed;

// Seed data for produktions-siden.
// Viser relationerne i modellen:
//  - Actor 1 spiller med i flere film (1, 3 og 5)
//  - Film 1 har flere skuespillere (1, 2 og 3)
//  - Film 6 er "Planned" uden director, production company, production record og screenings
//    -> viser at film kan oprettes før alt om produktionen er kendt
public static class MovieSeedData
{
    public static void SeedMovieData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Director>().HasData(Directors);
        modelBuilder.Entity<ProductionCompany>().HasData(ProductionCompanies);
        modelBuilder.Entity<Studio>().HasData(Studios);
        modelBuilder.Entity<Movie>().HasData(Movies);
        modelBuilder.Entity<ProductionRecord>().HasData(ProductionRecords);
        modelBuilder.Entity<Actor>().HasData(Actors);
        modelBuilder.Entity<MovieActor>().HasData(MovieActors);
        modelBuilder.Entity<Location>().HasData(Locations);
        modelBuilder.Entity<MovieLocation>().HasData(MovieLocations);
    }

    public static readonly Director[] Directors =
    {
        new Director { DirectorId = 1, Name = "Ida Mortensen", BirthDate = new DateOnly(1975, 3, 14), Nationality = "Danish" },
        new Director { DirectorId = 2, Name = "Lucas Ferreira", BirthDate = new DateOnly(1982, 7, 2), Nationality = "Brazilian" },
        new Director { DirectorId = 3, Name = "Hannah Weiss", BirthDate = new DateOnly(1979, 11, 23), Nationality = "German" },
        new Director { DirectorId = 4, Name = "Kenji Arata", BirthDate = new DateOnly(1968, 5, 9), Nationality = "Japanese" },
    };

    public static readonly ProductionCompany[] ProductionCompanies =
    {
        new ProductionCompany { ProductionCompanyId = 1, Name = "Nordic Frame Pictures", FoundingYear = 1994, Country = "Denmark" },
        new ProductionCompany { ProductionCompanyId = 2, Name = "Silverline Studios Group", FoundingYear = 2001, Country = "United Kingdom" },
        new ProductionCompany { ProductionCompanyId = 3, Name = "Meridian Films", FoundingYear = 1988, Country = "USA" },
    };

    public static readonly Studio[] Studios =
    {
        new Studio { StudioId = 1, ProductionCompanyId = 1, Name = "Nordic Frame Stages", City = "Copenhagen", Country = "Denmark", NumberOfSoundStages = 4, MaxStageCapacity = 300 },
        new Studio { StudioId = 2, ProductionCompanyId = 2, Name = "Silverline North Lot", City = "Manchester", Country = "United Kingdom", NumberOfSoundStages = 6, MaxStageCapacity = 450 },
        new Studio { StudioId = 3, ProductionCompanyId = 3, Name = "Meridian Sound City", City = "Atlanta", Country = "USA", NumberOfSoundStages = 10, MaxStageCapacity = 800 },
        new Studio { StudioId = 4, ProductionCompanyId = 3, Name = "Meridian West", City = "Vancouver", Country = "Canada", NumberOfSoundStages = 5, MaxStageCapacity = 400 },
    };

    // MovieId 1-5 bruges af screenings i CinemaSeedData
    public static readonly Movie[] Movies =
    {
        new Movie { MovieId = 1, Title = "The Last Lighthouse", ReleaseDate = new DateOnly(2026, 3, 12), RuntimeMinutes = 118, AgeRating = "15", Status = MovieStatus.Released, DirectorId = 1, ProductionCompanyId = 1 },
        new Movie { MovieId = 2, Title = "Echoes of Neon", ReleaseDate = new DateOnly(2026, 5, 22), RuntimeMinutes = 104, AgeRating = "11", Status = MovieStatus.Released, DirectorId = 2, ProductionCompanyId = 2 },
        new Movie { MovieId = 3, Title = "Glass Harbor", ReleaseDate = new DateOnly(2026, 8, 7), RuntimeMinutes = 126, AgeRating = "15", Status = MovieStatus.Released, DirectorId = 3, ProductionCompanyId = 1 },
        new Movie { MovieId = 4, Title = "Northbound", ReleaseDate = new DateOnly(2026, 9, 18), RuntimeMinutes = 98, AgeRating = "7", Status = MovieStatus.Released, DirectorId = 4, ProductionCompanyId = 3 },
        new Movie { MovieId = 5, Title = "Paper Satellites", ReleaseDate = new DateOnly(2026, 10, 2), RuntimeMinutes = 111, AgeRating = "A", Status = MovieStatus.Released, DirectorId = 1, ProductionCompanyId = 3 },
        new Movie { MovieId = 6, Title = "Winter Protocol", Status = MovieStatus.Planned },
    };

    // 1:1 - film 6 har bevidst intet production record
    public static readonly ProductionRecord[] ProductionRecords =
    {
        new ProductionRecord { ProductionRecordId = 1, MovieId = 1, Budget = 42_000_000m, ShootingStartDate = new DateOnly(2024, 9, 2), ShootingEndDate = new DateOnly(2025, 1, 31) },
        new ProductionRecord { ProductionRecordId = 2, MovieId = 2, Budget = 65_500_000m, ShootingStartDate = new DateOnly(2024, 11, 4), ShootingEndDate = new DateOnly(2025, 4, 18) },
        new ProductionRecord { ProductionRecordId = 3, MovieId = 3, Budget = 28_750_000m, ShootingStartDate = new DateOnly(2025, 2, 10), ShootingEndDate = new DateOnly(2025, 6, 27) },
        new ProductionRecord { ProductionRecordId = 4, MovieId = 4, Budget = 18_000_000m, ShootingStartDate = new DateOnly(2025, 4, 1), ShootingEndDate = new DateOnly(2025, 7, 15) },
        new ProductionRecord { ProductionRecordId = 5, MovieId = 5, Budget = 90_000_000m, ShootingStartDate = new DateOnly(2025, 3, 3), ShootingEndDate = new DateOnly(2025, 9, 12) },
    };

    public static readonly Actor[] Actors =
    {
        new Actor { ActorId = 1, Name = "Mikkel Strand", BirthDate = new DateOnly(1980, 2, 17), Nationality = "Danish", Gender = "Male" },
        new Actor { ActorId = 2, Name = "Clara Holt", BirthDate = new DateOnly(1988, 6, 30), Nationality = "Swedish", Gender = "Female" },
        new Actor { ActorId = 3, Name = "Oskar Brandt", BirthDate = new DateOnly(1955, 9, 8), Nationality = "German", Gender = "Male" },
        new Actor { ActorId = 4, Name = "Isabela Duarte", BirthDate = new DateOnly(1992, 1, 12), Nationality = "Brazilian", Gender = "Female" },
        new Actor { ActorId = 5, Name = "Marcus Reed", BirthDate = new DateOnly(1985, 4, 25), Nationality = "British", Gender = "Male" },
        new Actor { ActorId = 6, Name = "Freja Nygaard", BirthDate = new DateOnly(1990, 10, 3), Nationality = "Danish", Gender = "Female" },
        new Actor { ActorId = 7, Name = "Elena Petrova", BirthDate = new DateOnly(1995, 12, 19), Nationality = "Bulgarian", Gender = "Female" },
        new Actor { ActorId = 8, Name = "Daniel Okafor", BirthDate = new DateOnly(1983, 8, 14), Nationality = "Nigerian", Gender = "Male" },
        new Actor { ActorId = 9, Name = "Grace Whitfield", BirthDate = new DateOnly(1987, 3, 6), Nationality = "American", Gender = "Female" },
        new Actor { ActorId = 10, Name = "Yuki Tanabe", BirthDate = new DateOnly(1993, 7, 21), Nationality = "Japanese", Gender = "Female" },
    };

    public static readonly MovieActor[] MovieActors =
    {
        // Film 1 - flere skuespillere
        new MovieActor { MovieId = 1, ActorId = 1, CharacterName = "Captain Erik Holm", RoleType = RoleType.Lead },
        new MovieActor { MovieId = 1, ActorId = 2, CharacterName = "Sofie Lund", RoleType = RoleType.Supporting },
        new MovieActor { MovieId = 1, ActorId = 3, CharacterName = "The Keeper", RoleType = RoleType.Cameo },

        new MovieActor { MovieId = 2, ActorId = 4, CharacterName = "Maya Reyes", RoleType = RoleType.Lead },
        new MovieActor { MovieId = 2, ActorId = 5, CharacterName = "Detective Cole", RoleType = RoleType.Supporting },
        new MovieActor { MovieId = 2, ActorId = 2, CharacterName = "Dr. Anna Berg", RoleType = RoleType.Supporting },

        new MovieActor { MovieId = 3, ActorId = 1, CharacterName = "Jonas Vik", RoleType = RoleType.Lead },
        new MovieActor { MovieId = 3, ActorId = 6, CharacterName = "Harbor Master Ane", RoleType = RoleType.Supporting },
        new MovieActor { MovieId = 3, ActorId = 7, CharacterName = "Lena", RoleType = RoleType.Supporting },

        new MovieActor { MovieId = 4, ActorId = 8, CharacterName = "Tom Ashby", RoleType = RoleType.Lead },
        new MovieActor { MovieId = 4, ActorId = 9, CharacterName = "Nora Ashby", RoleType = RoleType.Lead },
        new MovieActor { MovieId = 4, ActorId = 5, CharacterName = "Gas Station Clerk", RoleType = RoleType.Cameo },

        // Actor 1 optræder i film 1, 3 og 5
        new MovieActor { MovieId = 5, ActorId = 1, CharacterName = "Professor Lind", RoleType = RoleType.Supporting },
        new MovieActor { MovieId = 5, ActorId = 10, CharacterName = "Ava Chen", RoleType = RoleType.Lead },
        new MovieActor { MovieId = 5, ActorId = 4, CharacterName = "Mission Control", RoleType = RoleType.Cameo },

        // Planlagt film med cast, men uden screenings
        new MovieActor { MovieId = 6, ActorId = 8, CharacterName = "Agent Marsh", RoleType = RoleType.Lead },
    };

    public static readonly Location[] Locations =
    {
        new Location { LocationId = 1, Name = "Hanstholm Lighthouse", City = "Hanstholm", Country = "Denmark", LocationType = LocationType.Building },
        new Location { LocationId = 2, Name = "Old Harbor Quay", City = "Esbjerg", Country = "Denmark", LocationType = LocationType.Street },
        new Location { LocationId = 3, Name = "Neon District Set", City = "Manchester", Country = "United Kingdom", LocationType = LocationType.Set },
        new Location { LocationId = 4, Name = "Lofoten Coastline", City = "Lofoten", Country = "Norway", LocationType = LocationType.Landscape },
        new Location { LocationId = 5, Name = "Desert Highway Stretch", City = "Kingman", Country = "USA", LocationType = LocationType.Street },
    };

    public static readonly MovieLocation[] MovieLocations =
    {
        new MovieLocation { MovieId = 1, LocationId = 1, ShootingDays = 12 },
        new MovieLocation { MovieId = 1, LocationId = 4, ShootingDays = 8 },
        new MovieLocation { MovieId = 2, LocationId = 3, ShootingDays = 30 },
        new MovieLocation { MovieId = 3, LocationId = 2, ShootingDays = 15 },
        new MovieLocation { MovieId = 3, LocationId = 4, ShootingDays = 6 },
        new MovieLocation { MovieId = 4, LocationId = 5, ShootingDays = 20 },
        new MovieLocation { MovieId = 5, LocationId = 3, ShootingDays = 10 },
    };
}
