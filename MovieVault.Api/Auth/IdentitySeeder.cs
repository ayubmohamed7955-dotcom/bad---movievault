
using Microsoft.AspNetCore.Identity;
using MovieVault.Api.Auth;
using Microsoft.EntityFrameworkCore;
using MovieVault.Api.Data;

namespace MovieVault.Api.Auth;

public static class IdentitySeeder
{
    // Opretter de fire roller
    public static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager)
    {
        string[] roles =
        {
            "Actor",
            "Director",
            "Reviewer",
            "Admin"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    // Opretter demo-brugere og tildeler roller
    public static async Task SeedUsersAsync(
        UserManager<ApiUser> userManager,
        IConfiguration configuration)
    {
        // reviewer2.demo bruger samme password-konfiguration som reviewer.demo (DemoUsers:Reviewer:Password).
        // To Reviewer-konti er nødvendige for at vise, at man ikke kan ændre andres anmeldelser.
        var demoUsers = new[]
        {
            new { UserName = "actor.demo", Role = "Actor" },
            new { UserName = "director.demo", Role = "Director" },
            new { UserName = "reviewer.demo", Role = "Reviewer" },
            new { UserName = "reviewer2.demo", Role = "Reviewer" },
            new { UserName = "admin.demo", Role = "Admin" }
        };

        foreach (var demo in demoUsers)
        {
            var user = await userManager.FindByNameAsync(
                demo.UserName);

            if (user == null)
            {
                var password = configuration[
                    $"DemoUsers:{demo.Role}:Password"];

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException(
                        $"Missing password for {demo.Role}.");
                }

                user = new ApiUser
                {
                    UserName = demo.UserName
                };

                var result = await userManager.CreateAsync(
                    user, password);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(user, demo.Role))
            {
                var result = await userManager.AddToRoleAsync(
                    user, demo.Role);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }
        }
    }


public static async Task LinkDemoUsersAsync(
    UserManager<ApiUser> userManager,
    MovieVaultDbContext db)
{
    // Find Identity-brugeren actor.demo
    var actorUser = await userManager.FindByNameAsync("actor.demo");

    // Find skuespilleren Mikkel Strand (ActorId = 1)
    var actor = await db.Actors.FindAsync(1);

    if (actorUser == null || actor == null)
    {
        throw new InvalidOperationException(
            "Demo Actor user or Actor record was not found.");
    }

    // Forbind skuespilleren med Identity-brugeren
    actor.UserId = actorUser.Id;

    // Find Identity-brugeren director.demo
    var directorUser = await userManager.FindByNameAsync("director.demo");

    // Find instruktøren Ida Mortensen (DirectorId = 1)
    var director = await db.Directors.FindAsync(1);

    if (directorUser == null || director == null)
    {
        throw new InvalidOperationException(
            "Demo Director user or Director record was not found.");
    }

    // Forbind instruktøren med Identity-brugeren
    director.UserId = directorUser.Id;

    // Forbind anmelderne med Identity-brugerne:
    // reviewer.demo -> Rita Reel (ReviewerId = 1), reviewer2.demo -> Jonas Frame (ReviewerId = 2)
    var reviewerLinks = new[]
    {
        new { UserName = "reviewer.demo", ReviewerId = 1 },
        new { UserName = "reviewer2.demo", ReviewerId = 2 }
    };

    foreach (var link in reviewerLinks)
    {
        var reviewerUser = await userManager.FindByNameAsync(link.UserName);
        var reviewer = await db.Reviewers.FindAsync(link.ReviewerId);

        if (reviewerUser == null || reviewer == null)
        {
            throw new InvalidOperationException(
                $"Demo Reviewer user {link.UserName} or Reviewer record {link.ReviewerId} was not found.");
        }

        reviewer.UserId = reviewerUser.Id;
    }

    // Gem ændringerne i SQL Server
    await db.SaveChangesAsync();
}


public static async Task SeedServiceAccountAsync(
    UserManager<ApiUser> userManager,
    IConfiguration configuration)
{
    // Hent servicekontoens oplysninger fra konfigurationen
    var userName = configuration["ServiceAccount:UserName"];
    var password = configuration["ServiceAccount:Password"];

    if (string.IsNullOrWhiteSpace(userName) ||
        string.IsNullOrWhiteSpace(password))
    {
        throw new InvalidOperationException(
            "Service account configuration is missing.");
    }

    // Kontrollér om servicekontoen allerede findes
    var user = await userManager.FindByNameAsync(userName);

    if (user == null)
    {
        user = new ApiUser
        {
            UserName = userName
        };

        // Identity håndterer password-hashing
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(", ",
                    result.Errors.Select(e => e.Description)));
        }
    } 

    // Giv servicekontoen Admin-rollen
    if (!await userManager.IsInRoleAsync(user, "Admin"))
    {
        var result = await userManager.AddToRoleAsync(user, "Admin");

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(", ",
                    result.Errors.Select(e => e.Description)));
        }
    }
}

}
