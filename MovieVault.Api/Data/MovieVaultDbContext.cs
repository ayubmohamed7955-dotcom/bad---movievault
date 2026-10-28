using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Data.Seed;
using MovieVault.Api.Entities;

namespace MovieVault.Api.Data;

public class MovieVaultDbContext : DbContext
{
    public MovieVaultDbContext(DbContextOptions<MovieVaultDbContext> options) : base(options)
    {
    }

    // Produktions-siden
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Director> Directors => Set<Director>();
    public DbSet<ProductionCompany> ProductionCompanies => Set<ProductionCompany>();
    public DbSet<Studio> Studios => Set<Studio>();
    public DbSet<ProductionRecord> ProductionRecords => Set<ProductionRecord>();
    public DbSet<Actor> Actors => Set<Actor>();
    public DbSet<MovieActor> MovieActors => Set<MovieActor>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<MovieLocation> MovieLocations => Set<MovieLocation>();

    // Biograf-siden
    public DbSet<Cinema> Cinemas => Set<Cinema>();
    public DbSet<Screen> Screens => Set<Screen>();
    public DbSet<Screening> Screenings => Set<Screening>();
    public DbSet<TicketSale> TicketSales => Set<TicketSale>();

    // Enums gemmes som tekst i databasen ("Released" i stedet for 2), så data kan læses direkte
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<MovieStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<RoleType>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<LocationType>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<ScreeningStatus>().HaveConversion<string>().HaveMaxLength(20);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Movie ----------
        modelBuilder.Entity<Movie>(e =>
        {
            e.Property(m => m.Title).IsRequired().HasMaxLength(200);
            e.Property(m => m.AgeRating).HasMaxLength(10);
            e.ToTable(t => t.HasCheckConstraint("CK_Movie_RuntimeMinutes", "[RuntimeMinutes] IS NULL OR [RuntimeMinutes] > 0"));

            // directs (1:N, optional): film kan oprettes uden director.
            // Slettes en director, beholdes filmen og DirectorId sættes til NULL.
            e.HasOne(m => m.Director)
                .WithMany(d => d.Movies)
                .HasForeignKey(m => m.DirectorId)
                .OnDelete(DeleteBehavior.SetNull);

            // produces (1:N, optional): samme logik som director
            e.HasOne(m => m.ProductionCompany)
                .WithMany(p => p.Movies)
                .HasForeignKey(m => m.ProductionCompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            // has (1:1, optional for Movie): FK MovieId ligger på ProductionRecord.
            // HasForeignKey<ProductionRecord> giver et UNIQUE index på MovieId, så 1:1 håndhæves i databasen.
            // Et production record giver ingen mening uden sin film -> Cascade.
            e.HasOne(m => m.ProductionRecord)
                .WithOne(r => r.Movie)
                .HasForeignKey<ProductionRecord>(r => r.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Director ----------
        modelBuilder.Entity<Director>(e =>
        {
            e.Property(d => d.Name).IsRequired().HasMaxLength(100);
            e.Property(d => d.Nationality).HasMaxLength(60);
        });

        // ---------- ProductionCompany ----------
        modelBuilder.Entity<ProductionCompany>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(150);
            e.Property(p => p.Country).HasMaxLength(60);
        });

        // ---------- Studio ----------
        modelBuilder.Entity<Studio>(e =>
        {
            e.Property(s => s.Name).IsRequired().HasMaxLength(150);
            e.Property(s => s.City).HasMaxLength(100);
            e.Property(s => s.Country).HasMaxLength(60);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Studio_NumberOfSoundStages", "[NumberOfSoundStages] IS NULL OR [NumberOfSoundStages] >= 0");
                t.HasCheckConstraint("CK_Studio_MaxStageCapacity", "[MaxStageCapacity] IS NULL OR [MaxStageCapacity] > 0");
            });

            // owns (1:N): et studio SKAL have en ejer. Et selskab med studier kan ikke slettes.
            e.HasOne(s => s.ProductionCompany)
                .WithMany(p => p.Studios)
                .HasForeignKey(s => s.ProductionCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- ProductionRecord ----------
        modelBuilder.Entity<ProductionRecord>(e =>
        {
            e.Property(r => r.Budget).HasPrecision(14, 2);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_ProductionRecord_Budget", "[Budget] IS NULL OR [Budget] >= 0");
                t.HasCheckConstraint("CK_ProductionRecord_ShootingDates",
                    "[ShootingStartDate] IS NULL OR [ShootingEndDate] IS NULL OR [ShootingEndDate] >= [ShootingStartDate]");
            });
        });

        // ---------- Actor ----------
        modelBuilder.Entity<Actor>(e =>
        {
            e.Property(a => a.Name).IsRequired().HasMaxLength(100);
            e.Property(a => a.Nationality).HasMaxLength(60);
            e.Property(a => a.Gender).HasMaxLength(20);
        });

        // ---------- MovieActor (features, N:N) ----------
        modelBuilder.Entity<MovieActor>(e =>
        {
            // Composite key: en skuespiller kan kun have én rolle per film
            e.HasKey(ma => new { ma.MovieId, ma.ActorId });
            e.Property(ma => ma.CharacterName).IsRequired().HasMaxLength(100);

            // Slettes film eller skuespiller, forsvinder rollen også
            e.HasOne(ma => ma.Movie)
                .WithMany(m => m.Cast)
                .HasForeignKey(ma => ma.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ma => ma.Actor)
                .WithMany(a => a.Roles)
                .HasForeignKey(ma => ma.ActorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Location ----------
        modelBuilder.Entity<Location>(e =>
        {
            e.Property(l => l.Name).IsRequired().HasMaxLength(150);
            e.Property(l => l.City).HasMaxLength(100);
            e.Property(l => l.Country).HasMaxLength(60);
        });

        // ---------- MovieLocation (filmed, N:N) ----------
        modelBuilder.Entity<MovieLocation>(e =>
        {
            e.HasKey(ml => new { ml.MovieId, ml.LocationId });
            e.ToTable(t => t.HasCheckConstraint("CK_MovieLocation_ShootingDays", "[ShootingDays] IS NULL OR [ShootingDays] > 0"));

            e.HasOne(ml => ml.Movie)
                .WithMany(m => m.FilmingLocations)
                .HasForeignKey(ml => ml.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ml => ml.Location)
                .WithMany(l => l.Movies)
                .HasForeignKey(ml => ml.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Cinema / Screen ----------
        modelBuilder.Entity<Screen>(e =>
        {
            e.ToTable(t => t.HasCheckConstraint("CK_Screen_SeatingCapacity", "[SeatingCapacity] > 0"));

            // To sale i samme biograf kan ikke have samme nummer
            e.HasIndex(s => new { s.CinemaId, s.ScreenNumber }).IsUnique();

            // contains (1:N): en biograf med sale kan ikke slettes
            e.HasOne(s => s.Cinema)
                .WithMany(c => c.Screens)
                .HasForeignKey(s => s.CinemaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Screening ----------
        modelBuilder.Entity<Screening>(e =>
        {
            e.ToTable(t => t.HasCheckConstraint("CK_Screening_TicketPrice", "[TicketPrice] >= 0"));
            e.HasIndex(s => s.StartTime); // bruges af "upcoming screenings"

            // shows (1:N): en film med screenings kan ikke slettes (beskytter salgsdata)
            e.HasOne(s => s.Movie)
                .WithMany(m => m.Screenings)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            // hosts (1:N): en sal med screenings kan ikke slettes
            e.HasOne(s => s.Screen)
                .WithMany(sc => sc.Screenings)
                .HasForeignKey(s => s.ScreenId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- TicketSale ----------
        modelBuilder.Entity<TicketSale>(e =>
        {
            e.ToTable(t => t.HasCheckConstraint("CK_TicketSale_TotalTickets", "[TotalTickets] > 0"));

            // sells (1:N): en screening med solgte billetter kan ikke slettes (annullér den i stedet)
            e.HasOne(ts => ts.Screening)
                .WithMany(s => s.TicketSales)
                .HasForeignKey(ts => ts.ScreeningId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Seed data ----------
        modelBuilder.SeedMovieData();
        modelBuilder.SeedCinemaData();
    }
}
