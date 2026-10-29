# MovieVault Web API

SW4BAD Mandatory Assignment 3 – Group 5

ASP.NET Core Web API (.NET 10) with Entity Framework Core and SQL Server. All POST, PUT and DELETE requests are logged to MongoDB with Serilog. The consolidated E/R diagram is included in the submission as `MovieVault-ER-diagram.pdf`.

## Group members

| Name | Student number |
|---|---|
| Ayub Mohamed Yusuf | au781514 |
| Mohamad Saleh Yassine | au759451 |
| Jewad Mahouk | au764175 |
| Mohamed Abdirahman Hassan Mahamod | au758453 |

## Contribution report

| Part | Responsible | Contribution |
|---|---|---|
| A. Consolidated E/R diagram | Jewad | Compared the individual diagrams and created the consolidated E/R diagram |
| B. Database implementation with EF Core | Jewad, Mohamed | Jewad: entity classes, primary keys, foreign keys and relationships. Mohamed: DbContext configuration, migrations and seed data |
| C. REST API endpoints | Mohamed | Movie CRUD, cast and filming location operations, screenings and ticket sales with validation |
| D. Query endpoints | Mohamad | Movie overview, movie details, upcoming screenings, ticket sales by movie and actor performance |
| E. Logging to MongoDB | Ayub | Serilog configuration, MongoDB sink and request logging middleware |
| F. OpenAPI + Scalar | Ayub | OpenAPI document, Scalar UI and XML documentation of endpoints |

All group members have reviewed the full solution and can explain every part of it.

## Running the application

### Prerequisites

- .NET 10 SDK
- SQL Server LocalDB (included with Visual Studio)
- MongoDB in Docker or installed locally (optional – without it, requests are only logged to the console)
- EF Core tools: `dotnet tool install --global dotnet-ef`

### Steps

Run from the solution folder:

```bash
# 1. Start MongoDB
docker run -d -p 27017:27017 --name movievault-mongo mongo

# 2. Create the database
dotnet ef database update --project MovieVault.Api

# 3. Run the API
dotnet run --project MovieVault.Api --launch-profile http
```

If the MongoDB container already exists, start it with `docker start movievault-mongo`.

## Database setup and migrations

- **Database:** SQL Server LocalDB, database `MovieVault`
- **Connection string:** `ConnectionStrings:DefaultConnection` in `MovieVault.Api/appsettings.json`
- **Migration:** `InitialCreate` in `MovieVault.Api/Migrations/`. It creates all tables, primary and foreign keys, check constraints and seed data.
- **Seed data:** added with `HasData` in `Data/Seed/`, so it is created together with the migration.
- **Log database:** MongoDB, database `MovieVaultLogs`, collection `RequestLogs` (configured under `Serilog` in `appsettings.json`)

Reset the database:

```bash
dotnet ef database drop --project MovieVault.Api --force
dotnet ef database update --project MovieVault.Api
```

**No LocalDB (e.g. not on Windows)?** Run SQL Server in Docker and override the connection string with user secrets:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_strong_Passw0rd" -p 1433:1433 --name movievault-sql -d mcr.microsoft.com/mssql/server:2022-latest
cd MovieVault.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=MovieVault;User Id=sa;Password=Your_strong_Passw0rd;TrustServerCertificate=True"
```

## URLs

| What | URL |
|---|---|
| API base URL | http://localhost:5019/api |
| Scalar UI | http://localhost:5019/scalar |
| OpenAPI JSON | http://localhost:5019/openapi/v1.json |
