using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using MovieVault.Api.Auth;
using MovieVault.Api.Data;
using MovieVault.Api.Middleware;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Filters;

var builder = WebApplication.CreateBuilder(args);

// ---------- Request logs (MongoDB) ----------
// Samme indstillinger bruges af Serilog (skriver) og LogsController (læser).
// Lokalt: appsettings.json. I Docker: miljøvariablen RequestLogging__MongoUrl.
var requestLogUrl = new MongoUrl(
    builder.Configuration["RequestLogging:MongoUrl"] ?? "mongodb://localhost:27017/MovieVaultLogs");
var requestLogDatabase = requestLogUrl.DatabaseName ?? "MovieVaultLogs";
var requestLogCollection = builder.Configuration["RequestLogging:CollectionName"] ?? "RequestLogs";

// Serilog: niveauer og konsol fra appsettings.json.
// Kun ChangeLoggingMiddlewares logs sendes til MongoDB.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Logger(requestLogs => requestLogs
        .Filter.ByIncludingOnly(Matching.FromSource<ChangeLoggingMiddleware>())
        .WriteTo.MongoDBBson(
            databaseUrl: requestLogUrl.Url,
            collectionName: requestLogCollection)));

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(requestLogUrl));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>()
    .GetDatabase(requestLogDatabase)
    .GetCollection<BsonDocument>(requestLogCollection));

// Add services to the container.

// Enums (fx ScreeningStatus) sendes som tekst i JSON i stedet for tal
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// EF Core med SQL Server - connection string fra appsettings.json / miljøvariabel
builder.Services.AddDbContext<MovieVaultDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDataProtection();

builder.Services
    .AddIdentityCore<ApiUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MovieVaultDbContext>()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is missing.");

var signingKey = Convert.FromBase64String(jwtKey);

if (signingKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT signing key must be at least 32 bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(signingKey),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<MovieVaultDbContext>();

    // Kør migrationer ved opstart (opretter databasen i Docker).
    // SQL Server-containeren kan være lidt længe om at blive klar, så vi prøver nogle gange.
    await MigrateWithRetryAsync(db, app.Logger);

    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApiUser>>();

    // Opret roller
    await IdentitySeeder.SeedRolesAsync(roleManager);

    // Opret demo-brugere
    await IdentitySeeder.SeedUsersAsync(
        userManager,
        app.Configuration);

    // Forbind demo-brugere med Actor og Director
    await IdentitySeeder.LinkDemoUsersAsync(userManager, db);

    await IdentitySeeder.SeedServiceAccountAsync(
        userManager,
        app.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();

// Efter UseAuthentication: middlewaren kender den indloggede bruger.
// Før UseAuthorization: 401 og 403 fra authorization bliver også logget.
app.UseMiddleware<ChangeLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task MigrateWithRetryAsync(MovieVaultDbContext db, Microsoft.Extensions.Logging.ILogger logger)
{
    const int maxAttempts = 10;

    for (var attempt = 1; ; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning("Database not ready (attempt {Attempt}/{MaxAttempts}): {Message}. Retrying in 5 seconds.",
                attempt, maxAttempts, ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
