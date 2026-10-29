# MovieVault Web API

SW4BAD Mandatory Assignment 3 – Group 5

MovieVault is an ASP.NET Core Web API (.NET 10) that manages a movie catalogue and connects the production side of the movie industry (directors, production companies, studios, actors, filming locations) with cinemas, screenings and ticket sales. Data is stored in SQL Server through Entity Framework Core, and all changes made through the API (POST, PUT, DELETE) are logged to MongoDB with Serilog.

## Group members

| Name | Student number |
|---|---|
| Ayub Mohamed Yusuf | [STUDENT NUMBER] |
| Mohamad Saleh Yassine | au759451 |
| Jewad Mahouk | [STUDENT NUMBER] |
| Mohamed Abdirahman Hassan Mahamod | [STUDENT NUMBER] |

## Contribution report

| Part | Responsible | Contribution |
|---|---|---|
| A. Consolidated E/R diagram | [NAME] | [CONTRIBUTION] |
| B. Database implementation with EF Core | [NAME] | [CONTRIBUTION] |
| C. REST API endpoints | [NAME] | [CONTRIBUTION] |
| D. Query endpoints | Mohamad Saleh Yassine | au759451 |
| E. Logging to MongoDB | [NAME] | [CONTRIBUTION] |
| F. OpenAPI + Scalar | [NAME] | [CONTRIBUTION] |

Everyone in the group has reviewed the full solution and can explain all parts of it.

## E/R diagram

The consolidated E/R diagram (Chen notation) is included in the submission as `MovieVault-ER-diagram.pdf`.

### Relationships

- **1:1** – Movie ↔ ProductionRecord. The foreign key `MovieId` is on ProductionRecord with a unique index, which is what makes it 1:1 instead of 1:N.
- **1:N** – Director → Movie, ProductionCompany → Movie, ProductionCompany → Studio, Cinema → Screen, Screen → Screening, Movie → Screening, Screening → TicketSale.
- **N:N** – Movie ↔ Actor through `MovieActor` (with `CharacterName` and `RoleType`), and Movie ↔ Location through `MovieLocation` (with `ShootingDays`). Both junction tables use a composite primary key, e.g. `(MovieId, ActorId)`, so an actor can only have one role per movie.

### Modelling decisions

Our individual diagrams had the same entities and relationships but differed mainly in which participations were optional. We decided based on the requirements from MovieVault:

- **Director, production company and production record are optional on a movie**, because movies may be created before all production information is known. In code this is a nullable foreign key (`int? DirectorId`).
- **A movie may exist without screenings**, but **a screening must refer to a movie and a screen**, and **a screen must belong to a cinema** (non-nullable foreign keys).
- **A studio must be owned by a production company**, while a production company may own zero studios.
- **Screenings have a `Status` (Scheduled/Cancelled)**, so a screening can be cancelled without deleting it and losing its ticket sales.
- **Screening date and time are combined in one `StartTime`** field, which makes chronological sorting simple.
- **Fixed values are enums**: `MovieStatus`, `RoleType` (Lead/Supporting/Cameo), `LocationType` and `ScreeningStatus`. They are stored as text in the database.
- **Seating capacity is required**, since the capacity check depends on it.
- **Revenue is not stored.** It is calculated as `TotalTickets × TicketPrice`, so it can never become inconsistent.
- We used **unique relationship names** (directs, produces, has, features, filmed, shows, contains, hosts, sells, owns) to keep the diagram unambiguous.

## Technology

- .NET 10, ASP.NET Core Web API (controllers)
- Entity Framework Core with SQL Server (LocalDB)
- Serilog with MongoDB sink
- OpenAPI + Scalar

## How to run

### Prerequisites

- .NET 10 SDK
- SQL Server LocalDB (included with Visual Studio, or installed from SQL Server Express → "Download Media" → "LocalDB"). LocalDB requires Windows.
- MongoDB, either in Docker or installed locally
- EF Core tools: `dotnet tool install --global dotnet-ef`

### 1. Start MongoDB

With Docker:

```bash
docker run -d -p 27017:27017 --name movievault-mongo mongo
```

If the container already exists: `docker start movievault-mongo`

If MongoDB is installed locally instead, make sure the MongoDB service is running on `localhost:27017`.

The API also runs without MongoDB – the requests are then only logged to the console.

### 2. Create the database

Run from the solution folder:

```bash
dotnet ef database update --project MovieVault.Api
```

This applies the migration `InitialCreate`, which creates all tables, keys, constraints and seed data in the LocalDB database `MovieVault`.

### 3. Run the API

```bash
dotnet run --project MovieVault.Api --launch-profile http
```

### URLs

| What | URL |
|---|---|
| API base URL | http://localhost:5019/api |
| Scalar UI | http://localhost:5019/scalar |
| OpenAPI JSON | http://localhost:5019/openapi/v1.json |

### Configuration

Both connection strings are in `MovieVault.Api/appsettings.json`:

- SQL Server: `ConnectionStrings:DefaultConnection` (`Server=(localdb)\\MSSQLLocalDB;Database=MovieVault;...`)
- MongoDB: `Serilog:WriteTo` → `MongoDBBson` → `databaseUrl` (`mongodb://localhost:27017/MovieVaultLogs`)

**Not on Windows / no LocalDB?** Run SQL Server in Docker and override the connection string locally with user secrets (not committed to Git):

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_strong_Passw0rd" -p 1433:1433 --name movievault-sql -d mcr.microsoft.com/mssql/server:2022-latest
cd MovieVault.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=MovieVault;User Id=sa;Password=Your_strong_Passw0rd;TrustServerCertificate=True"
```

### Reset the database

```bash
dotnet ef database drop --project MovieVault.Api --force
dotnet ef database update --project MovieVault.Api
```

### Inspect the database

In VS Code with the SQL Server (mssql) extension: add a connection with server `(localdb)\MSSQLLocalDB`, authentication **Windows Authentication**, and leave the database name empty. The `MovieVault` database appears under *Databases*.

## Database implementation with EF Core

- Each entity in the E/R diagram is a class in `Entities/`, and each table has a `DbSet` in `MovieVaultDbContext`.
- **Primary keys** follow the convention `<EntityName>Id` (e.g. `MovieId`), which EF Core recognises automatically and creates as identity columns.
- **1:N relationships** have a foreign key property on the "many" side and a collection navigation on the "one" side, e.g. `Screening.MovieId` and `Movie.Screenings`.
- **Optional relationships** use nullable foreign keys (`int?`), mandatory ones non-nullable (`int`).
- **Fluent API** in `OnModelCreating` configures what EF Core cannot infer: composite keys on the junction tables, the 1:1 relationship, decimal precision, enum-to-string conversion, string lengths, check constraints and delete behaviour.
- **Seed data** is added with `HasData` and is part of the migration.

### Delete behaviour

| Relationship | Behaviour | Reason |
|---|---|---|
| Movie → Screening, Screen → Screening, Screening → TicketSale, Cinema → Screen, ProductionCompany → Studio | Restrict | Protects ticket sales and mandatory relationships – cancel instead of delete |
| Movie → MovieActor, Movie → MovieLocation, Movie → ProductionRecord | Cascade | These have no meaning without the movie |
| Director → Movie, ProductionCompany → Movie | Set null | The relationship is optional, so the movie can remain |

## Endpoints

### Movies (full CRUD)

| Method | Route | Description |
|---|---|---|
| GET | `/api/movies` | **Query 1** – Movie overview (title, release date, status, director, production company, budget) |
| GET | `/api/movies/{id}` | **Query 2** – Movie details (basic info, director, production company, budget, cast with roles, filming locations) |
| POST | `/api/movies` | Create a movie (production record is optional) |
| PUT | `/api/movies/{id}` | Update a movie (replaces the whole movie, including the production record) |
| DELETE | `/api/movies/{id}` | Delete a movie (409 if it has screenings) |

### Cast and filming locations

Cast and filming locations are modelled as sub-resources of a movie. Each operation works on the junction tables `MovieActor` and `MovieLocation` – removing an actor from a movie deletes the role, not the actor.

| Method | Route | Description |
|---|---|---|
| GET | `/api/movies/{movieId}/cast` | Get the cast of a movie |
| POST | `/api/movies/{movieId}/cast` | Assign an actor with character name and role type (409 if already in the cast) |
| DELETE | `/api/movies/{movieId}/cast/{actorId}` | Remove an actor from a movie |
| GET | `/api/movies/{movieId}/locations` | Get filming locations of a movie |
| POST | `/api/movies/{movieId}/locations` | Associate a filming location with a movie (409 if already associated) |
| DELETE | `/api/movies/{movieId}/locations/{locationId}` | Remove a filming location from a movie |

### Actors

| Method | Route | Description |
|---|---|---|
| GET | `/api/actors` | Get all actors |
| GET | `/api/actors/{actorId}/performance` | **Query 5** – Movies the actor appears in and total tickets sold |

### Screenings

| Method | Route | Description |
|---|---|---|
| GET | `/api/screenings/{id}` | Get one screening including tickets sold and available seats |
| GET | `/api/movies/{movieId}/screenings` | Get screenings for a movie |
| GET | `/api/cinemas/{cinemaId}/screenings` | Get screenings for a cinema |
| GET | `/api/screenings/upcoming` | **Query 3** – Upcoming screenings, ordered chronologically |
| POST | `/api/screenings` | Create a screening for a movie on a screen |
| PUT | `/api/screenings/{id}` | Update a screening, or cancel it with `"status": "Cancelled"` |
| DELETE | `/api/screenings/{id}` | Delete a screening (409 if it has ticket sales) |

### Ticket sales (full CRUD)

| Method | Route | Description |
|---|---|---|
| GET | `/api/ticket-sales` | Get all ticket sales (optional `?screeningId=`) |
| GET | `/api/ticket-sales/{id}` | Get one ticket sale |
| POST | `/api/ticket-sales` | Register a ticket sale (one sale can contain several tickets) |
| PUT | `/api/ticket-sales/{id}` | Change the number of tickets |
| DELETE | `/api/ticket-sales/{id}` | Delete a ticket sale |
| GET | `/api/ticket-sales/by-movie` | **Query 4** – Tickets sold and revenue per movie |

### Status codes

| Code | Used when |
|---|---|
| 200 OK | Successful GET or PUT |
| 201 Created | Successful POST (with a `Location` header pointing to the new resource) |
| 204 No Content | Successful DELETE |
| 400 Bad Request | Invalid input, or an id in the request body that does not exist |
| 404 Not Found | The resource in the URL does not exist |
| 409 Conflict | The request conflicts with existing data (duplicate cast member, deleting a movie with screenings, etc.) |

Errors are returned as `ProblemDetails` / `ValidationProblemDetails` with a meaningful message.

## DTOs

EF entities are never exposed directly. All requests and responses use DTOs (`DTOs/`), because:

- **Input control** – the client can only send the fields it is allowed to set (e.g. not ids or `SaleDate`).
- **Output control** – responses contain exactly what the client needs, e.g. `DirectorName` instead of the full Director entity.
- **No circular references** – Movie → Screenings → Movie would otherwise cause an endless JSON loop.
- **Decoupling** – entities can change without breaking the API contract.
- **Validation** – request DTOs carry the validation rules, which are also shown in the OpenAPI documentation.

## Validation and business rules

Validation is done in three layers:

| Layer | Checks |
|---|---|
| **DTOs** (DataAnnotations, automatic 400 via `[ApiController]`) | The shape of the input: required fields, string lengths, ranges, valid enum values |
| **Controllers** (against the database) | Referenced data exists, duplicates, capacity, screening status |
| **Database** (check constraints) | Last line of defence: `TotalTickets > 0`, `TicketPrice >= 0`, `SeatingCapacity > 0`, `ShootingDays >= 0` |

Rules:

- Ticket quantity must be at least 1.
- Ticket price cannot be negative.
- A ticket sale cannot make a screening exceed the seating capacity of its screen. When a sale is updated, its own previous quantity is subtracted before checking.
- Tickets cannot be sold for cancelled screenings or screenings that have already started.
- Ticket sales run in a serializable transaction, so two simultaneous purchases cannot overbook a screen.
- Two scheduled screenings cannot overlap on the same screen (movie runtime + 15 minutes cleaning buffer).
- A screening cannot be moved to a screen that is too small for the tickets already sold.
- A screening with ticket sales cannot be deleted – it must be cancelled instead.
- A movie with screenings cannot be deleted. Its cast, filming locations and production record are deleted automatically (cascade).
- Director and production company must exist if they are given when creating or updating a movie.
- Shooting end date cannot be before the shooting start date.
- An actor can only have one role per movie, and a location can only be associated once per movie (composite keys).
- The server sets `SaleDate` – the client cannot.

## Query endpoints – how the data is retrieved

All queries use LINQ projections (`Select`) directly into DTOs. EF Core translates the navigation properties into JOINs and only selects the columns the DTO needs. Read-only queries use `AsNoTracking()`.

| Query | Path through the model | LINQ → SQL |
|---|---|---|
| 1. Movie overview | Movie → Director, ProductionCompany, ProductionRecord | Navigation → **LEFT JOIN** (the relationships are optional, so movies without a director are still included) |
| 2. Movie details | Movie → MovieActor → Actor, Movie → MovieLocation → Location | Nested `Select` through the junction tables → **JOIN** |
| 3. Upcoming screenings | Screening → Movie, Screening → Screen → Cinema | `Where` (start time in the future, not cancelled) → **WHERE**, `OrderBy(StartTime)` → **ORDER BY** |
| 4. Ticket sales by movie | Movie → Screening → TicketSale | `GroupBy` → **GROUP BY**, `Sum(TotalTickets)` and `Sum(TotalTickets × TicketPrice)` → **SUM** |
| 5. Actor performance | Actor → MovieActor → Movie → Screening → TicketSale | `SelectMany` flattens all ticket sales of a movie's screenings, `Sum` → **SUM**, number of movies → **COUNT** |

In Query 5, ticket sales for cancelled screenings are not counted.

## Logging

All POST, PUT and DELETE requests are logged to MongoDB by `ChangeLoggingMiddleware`. The middleware decides **what** is logged; the Serilog configuration in `appsettings.json` decides **where** it is stored.

How it works:

1. The middleware sees every request. GET requests are passed on without logging.
2. For POST, PUT and DELETE it calls the next step in the pipeline and waits for the response.
3. In a `finally` block it logs the request, so failed requests (500) are logged too.

Each log entry contains:

- HTTP method
- Request path
- Response status code
- Timestamp (UTC)
- Elapsed time in milliseconds

The values are stored as separate structured properties, so the logs can be filtered, e.g. on status code.

Logs are stored in the database `MovieVaultLogs`, collection `RequestLogs`. A filter in `appsettings.json` sends only the middleware's log entries to MongoDB, so framework and EF Core logs do not end up there.

View the latest logs (Docker):

```bash
docker exec movievault-mongo mongosh MovieVaultLogs --quiet --eval "db.RequestLogs.find().sort({_id:-1}).limit(5).toArray()"
```

Or connect with MongoDB Compass to `mongodb://localhost:27017` and open `MovieVaultLogs` → `RequestLogs`.

## Seed data

| Entity | Count |
|---|---|
| Movies | 6 |
| Actors | 10 |
| Directors | 4 |
| Production companies | 3 |
| Studios | 4 |
| Filming locations | 5 |
| Production records | 5 |
| Cinemas | 3 |
| Screens | 7 |
| Screenings | 12 |
| Ticket sales | 19 |

The seed data demonstrates the relationships in the model:

- Actor 1 (Mikkel Strand) appears in three movies, and several movies have multiple actors.
- Movies have screenings across all three cinemas.
- Movie 6 ("Winter Protocol") is planned and has no director, production company, production record or screenings – showing that movies can be created before production details are known.
- Screening 6 is almost sold out (38 of 40 seats), which makes it easy to test the capacity check.
- Screening 12 is cancelled and therefore not shown in upcoming screenings.
- The seed contains both past and upcoming screenings, so the upcoming filter can be tested.

## Project structure

```
MovieVault.Api/
├── Controllers/     API endpoints
├── DTOs/            Request and response models
├── Data/            DbContext and seed data
├── Entities/        EF Core entity classes and enums
├── Middleware/      Request logging middleware
├── Migrations/      EF Core migrations
└── Program.cs       Service and pipeline configuration
```