# MovieVault Web API

SW4BAD Mandatory Assignment 4 – Group 5. The updated E/R diagram is included as `MovieVault-ER-diagram.pdf`.

## Group members

| Name | Student number |
|---|---|
| Ayub Mohamed Yusuf | au781514 |
| Mohamad Saleh Yassine | au759451 |
| Jewad Mahouk | au764175 |
| Mohamed Abdirahman Hassan Mahamod | au758453 |

## Contribution report

| Part | Responsible |
|---|---|
| 1. Authentication and authorization | Jewad |
| 2. Reviews, media outlets and E/R diagram | Mohamed |
| 3. Ticket sale simulator | Mohamad |
| 4. Request logs and `GET /api/logs` | Ayub |
| 5. Postman tests | Mohamad |
| 7. Docker | Ayub |

All group members have reviewed the full solution.

## Setup and run

1. Copy `.env.example` to `.env` and fill in your own values. `.env` is ignored by Git and must not be committed. The JWT key must be Base64 and at least 32 bytes, e.g. generated in PowerShell with `[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Max 256 }))`.
2. Start the full system (API, simulator, SQL Server, MongoDB):

   ```bash
   docker compose up --build
   ```

3. Stop with `docker compose down`. `docker compose down -v` also deletes all data.

To run the API locally without Docker, set the same values with `dotnet user-secrets` in `MovieVault.Api` (`Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `DemoUsers:<Role>:Password`, `ServiceAccount:UserName`, `ServiceAccount:Password`) and run `dotnet run --project MovieVault.Api --launch-profile http`.

## URLs

| | Docker | Local |
|---|---|---|
| API base URL | http://localhost:8080/api | http://localhost:5019/api |
| Scalar UI | http://localhost:8080/scalar | http://localhost:5019/scalar |
| OpenAPI JSON | http://localhost:8080/openapi/v1.json | http://localhost:5019/openapi/v1.json |

## Migrations and seeding

- Migrations: `InitialCreate`, `AddIdentity`, `LinkActorsDirectorsToIdentity`, `AddReviewsAndMediaOutlets`.
- The API applies all migrations automatically at startup (`Database.MigrateAsync()`, retried while SQL Server starts). Manually: `dotnet ef database update --project MovieVault.Api`.
- Domain data is seeded through the migrations (`HasData`). Roles, demo users and their links to Actor/Director/Reviewer are seeded at startup by `IdentitySeeder`.
- Startup readiness: the API waits for the SQL Server and MongoDB health checks. The simulator retries until the API responds.
- Persistent storage: Docker volumes `sqlserver-data` (business data) and `mongodb-data` (request logs).

## Roles and demo accounts

| Username | Role | Linked to | Password (`.env`) |
|---|---|---|---|
| `admin.demo` | Admin | – | `DEMO_ADMIN_PASSWORD` |
| `actor.demo` | Actor | Mikkel Strand (movies 1, 3, 5) | `DEMO_ACTOR_PASSWORD` |
| `director.demo` | Director | Ida Mortensen (movies 1, 5) | `DEMO_DIRECTOR_PASSWORD` |
| `reviewer.demo` | Reviewer | Rita Reel, The Morning Ledger | `DEMO_REVIEWER_PASSWORD` |
| `reviewer2.demo` | Reviewer | Jonas Frame, Channel Nord | `DEMO_REVIEWER_PASSWORD` |
| `simulator.service` | Admin | Service account for the simulator | `SERVICE_ACCOUNT_PASSWORD` |

Actor, Director and Reviewer have a unique `UserId` linking them to the Identity user. If an account has multiple roles, Admin always sees all movies; otherwise the first matching role in the order Actor → Director → Reviewer decides. All demo accounts have one role.

## Simulator configuration

| Setting | Default | Description |
|---|---|---|
| `MovieVaultApi__BaseUrl` | `http://api:8080` in Docker | API address (Compose service name) |
| `ServiceAccount__UserName` / `__Password` | from `.env` | Admin service account |
| `Simulator__IntervalSeconds` | `15` | Seconds between simulated sales |
| `Simulator__MaxTicketsPerSale` | `4` | Max tickets per sale |

## Business rules and assumptions

- Available seats = screen capacity − sum of tickets sold. Creating or increasing a sale beyond capacity returns 400; deleting or reducing a sale frees seats.
- Revenue = tickets × the screening's current ticket price. It is calculated, not stored, so changing or deleting a sale changes revenue immediately. Simulated sales are ordinary ticket sales and are included.
- A cancelled screening keeps its sales, but no new tickets can be sold and it is excluded from upcoming screenings and sales queries.
- A screening with ticket sales or a movie with screenings cannot be deleted (409). Deleting a movie deletes its cast, filming locations, production record and reviews.
- A reviewer represents one media outlet. A review stores the outlet it was published in.

## Demonstration order

1. `docker compose up --build`, open Scalar.
2. Login as `admin.demo`; `GET /api/movies` without token → 401.
3. `GET /api/movies` as Admin, Actor and Director (different results); Actor `GET /api/movies/2` → 403.
4. Reviews: public `GET /api/reviews?movieId=1`; `reviewer.demo` `PUT /api/reviews/1` → 200, `PUT /api/reviews/2` → 403.
5. Simulator: `docker compose logs -f simulator`, then `GET /api/ticket-sales/by-movie`.
6. Logs: `GET /api/logs?username=simulator.service&requestType=POST` as Admin.

## Postman tests

Import `postman/MovieVault.postman_collection.json` and `postman/MovieVault.postman_environment.json`. Fill in `adminPassword` and `actorPassword`, set `baseUrl` (`http://localhost:8080`), and run the collection in this order:

1. Login
2. Authentication - 401
3. Movie CRUD - Admin
4. Authorization - Actor 403
5. Movie CRUD - Admin delete
6. Role-based movie access
