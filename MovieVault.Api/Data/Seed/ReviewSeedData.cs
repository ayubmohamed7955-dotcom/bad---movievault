using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Entities;

namespace MovieVault.Api.Data.Seed;

// Seed data for anmeldelser.
// Viser:
//  - Film 1 har flere anmeldelser fra forskellige medier (review 1, 2 og 3)
//  - The Morning Ledger har flere anmeldelser af forskellige film og fra to forskellige anmeldere
//  - Ejerskab: reviewer.demo (Rita Reel) ejer review 1, 4 og 7; reviewer2.demo (Jonas Frame) ejer review 2 og 6
//  - Film 6 har ingen anmeldelser, men er stadig i kataloget
// Anmelderne forbindes til Identity-brugere i IdentitySeeder.LinkDemoUsersAsync (UserId kendes først ved runtime).
public static class ReviewSeedData
{
    public static void SeedReviewData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaOutlet>().HasData(MediaOutlets);
        modelBuilder.Entity<Reviewer>().HasData(Reviewers);
        modelBuilder.Entity<Review>().HasData(Reviews);
    }

    public static readonly MediaOutlet[] MediaOutlets =
    {
        new MediaOutlet { MediaOutletId = 1, Name = "The Morning Ledger", OutletType = MediaOutletType.Newspaper, Website = "https://morningledger.example" },
        new MediaOutlet { MediaOutletId = 2, Name = "Channel Nord", OutletType = MediaOutletType.TvChannel, Website = "https://channelnord.example" },
        new MediaOutlet { MediaOutletId = 3, Name = "Screenwise Magazine", OutletType = MediaOutletType.Magazine, Website = "https://screenwise.example" },
    };

    public static readonly Reviewer[] Reviewers =
    {
        new Reviewer { ReviewerId = 1, Name = "Rita Reel", MediaOutletId = 1 },   // reviewer.demo
        new Reviewer { ReviewerId = 2, Name = "Jonas Frame", MediaOutletId = 2 }, // reviewer2.demo
        new Reviewer { ReviewerId = 3, Name = "Maja Lens", MediaOutletId = 1 },   // ingen login
        new Reviewer { ReviewerId = 4, Name = "Peter Cutts", MediaOutletId = 3 }, // ingen login
    };

    public static readonly Review[] Reviews =
    {
        new Review
        {
            ReviewId = 1, MovieId = 1, ReviewerId = 1, MediaOutletId = 1, Rating = 5,
            Headline = "A quiet storm of a film",
            Body = "The Last Lighthouse builds its tension slowly and never lets go. Mikkel Strand carries the film with a restrained, powerful lead performance.",
            PublishedAt = new DateTime(2026, 3, 13, 8, 0, 0, DateTimeKind.Utc)
        },
        new Review
        {
            ReviewId = 2, MovieId = 1, ReviewerId = 2, MediaOutletId = 2, Rating = 4,
            Headline = "Beautiful, if a little long",
            Body = "Stunning coastal photography and a strong cast. The middle act drags, but the ending is worth the wait.",
            PublishedAt = new DateTime(2026, 3, 14, 18, 30, 0, DateTimeKind.Utc)
        },
        new Review
        {
            ReviewId = 3, MovieId = 1, ReviewerId = 4, MediaOutletId = 3, Rating = 3,
            Headline = "Atmosphere over story",
            Body = "Few films look this good, but the plot is thin. Worth seeing on a big screen for the visuals alone.",
            PublishedAt = new DateTime(2026, 3, 20, 10, 0, 0, DateTimeKind.Utc)
        },
        new Review
        {
            ReviewId = 4, MovieId = 2, ReviewerId = 1, MediaOutletId = 1, Rating = 3,
            Headline = "Style without much substance",
            Body = "Echoes of Neon is a feast for the eyes, but the mystery is easy to solve long before the detectives do.",
            PublishedAt = new DateTime(2026, 5, 23, 7, 45, 0, DateTimeKind.Utc)
        },
        new Review
        {
            ReviewId = 5, MovieId = 3, ReviewerId = 3, MediaOutletId = 1, Rating = 4,
            Headline = "A harbour full of secrets",
            Body = "Glass Harbor is a confident small-town drama with an excellent ensemble and a script that trusts its audience.",
            PublishedAt = new DateTime(2026, 8, 8, 9, 15, 0, DateTimeKind.Utc)
        },
        new Review
        {
            ReviewId = 6, MovieId = 5, ReviewerId = 2, MediaOutletId = 2, Rating = 2,
            Headline = "Lost in orbit",
            Body = "Big ideas and a big budget, but Paper Satellites never decides what kind of film it wants to be.",
            PublishedAt = new DateTime(2026, 10, 3, 19, 0, 0, DateTimeKind.Utc)
        },
        new Review
        {
            ReviewId = 7, MovieId = 4, ReviewerId = 1, MediaOutletId = 1, Rating = 4,
            Headline = "A warm road movie",
            Body = "Northbound is funny, touching and refreshingly short. A good choice for the whole family.",
            PublishedAt = new DateTime(2026, 9, 19, 8, 0, 0, DateTimeKind.Utc)
        },
    };
}
