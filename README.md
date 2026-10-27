# SkillHub

SkillHub is a training center that offers short, in-person workshops (photography, cooking, public speaking,
design and more). This solution replaces the spreadsheets SkillHub used to track enrollments. It has:

- a **REST API** (`SkillHub.Api`) where employees publish workshops, see who is enrolled, bulk-enroll people from Excel
  and look at reports, and where participants enroll, cancel and download their certificates;
- a **web application for participants** (`SkillHub.Web`, ASP.NET Core Razor Pages, option **A**). It is a second
  presentation layer inside the same solution and calls the **application services** directly, not the API endpoints.

The business rules (seat limits, duplicates, schedule overlaps, the 24-hour cancellation window, re-enrollment)
live in the **domain** and **application** layers. Neither presentation layer contains them.

## Architecture

```
src/
├── SkillHub.Domain           Entities and business rules (Workshop, Enrollment, Instructor, Participant)
├── SkillHub.Application      Use cases (services), DTOs, abstractions (repositories, PDF, Excel, identity, clock)
├── SkillHub.Infrastructure   EF Core + MySQL, ASP.NET Core Identity, JWT, repositories, QuestPDF, ClosedXML, seeding
├── SkillHub.Api              REST API (controllers, JWT authentication, error handling, OpenAPI + Scalar)
└── SkillHub.Web              Razor Pages app for participants (cookie authentication)
tests/
└── SkillHub.Tests            Unit tests of the domain rules (xUnit)
```

Dependencies point inward: `Api`/`Web` → `Application` ← `Infrastructure`, and `Application` → `Domain`.
The presentation projects reference `Infrastructure` only to register its implementations in the DI container.

Design patterns and decisions:

- **Dependency injection** with the built-in .NET container. Classes depend on interfaces such as `IWorkshopRepository`,
  `ICertificateGenerator`, `IEnrollmentImportReader`, `IIdentityService`, `ITokenService` and `IDateTimeProvider`.
- **Repository** and **Unit of Work** (`IUnitOfWork`) over EF Core.
- **DTOs** for every input and output. Entities never leave the application layer.
- **Rich domain model**: `Workshop.Enroll(...)` and `Enrollment.Cancel(...)` enforce the rules and throw a
  `DomainException` when one is broken.
- **Optimistic concurrency**: `Workshop.Version` is a concurrency token that changes whenever the active enrollments
  of a workshop change, so two requests at the same moment can't both take the last seat.
- **Centralized error handling**: an `IExceptionHandler` maps exceptions to RFC 7807 `ProblemDetails` responses.

| Exception                         | HTTP status                |
|-----------------------------------|----------------------------|
| `ValidationException`             | 400 Bad Request            |
| `UnauthorizedException`           | 401 Unauthorized           |
| `ForbiddenException`              | 403 Forbidden              |
| `NotFoundException`               | 404 Not Found              |
| `ConflictException`               | 409 Conflict               |
| `DomainException` (business rule) | 422 Unprocessable Entity   |

## Technologies

- .NET 10 / ASP.NET Core 10 (Web API + Razor Pages)
- Entity Framework Core 9 (Code First + migrations) with **MySQL 8** (Pomelo provider)
- ASP.NET Core Identity (users and roles)
- JWT Bearer authentication (API) and cookie authentication (web)
- OpenAPI + **Scalar** for API documentation and testing
- **QuestPDF** for the enrollment certificate
- **ClosedXML** to read Excel files (seed data and bulk enrollment)
- xUnit for unit tests
- Docker / Docker Compose (optional)
- Bootstrap 5 for the web UI

## Getting started

### Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- MySQL 8 (local install **or** Docker)
- The `SkillHub_Data.xlsx` file

### 1. Seed file

The file is already included at [`data/SkillHub_Data.xlsx`](data).

On startup the application applies the migrations and loads the **Instructors**, **Participants** and **Workshops**
sheets. Instructors become employee accounts and participants become participant accounts. Seeding is idempotent:
rows that already exist (same email or code) are skipped, so restarting is safe. The **Enrollments** sheet is
**not** seeded. It is loaded only through `POST /api/v1/enrollments/import`.

### 2. Database

Option A: **MySQL in Docker** (exposed on port `3307` so it doesn't clash with a local MySQL):

```bash
docker compose up -d mysql
```

Then point both apps to it, either in `appsettings.json` or with an environment variable:

```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Port=3307;Database=skillhub;User=root;Password=root;"
```

Option B: **local MySQL**. Edit `ConnectionStrings:DefaultConnection` in
`src/SkillHub.Api/appsettings.json` and `src/SkillHub.Web/appsettings.json`:

```json
"DefaultConnection": "Server=localhost;Port=3306;Database=skillhub;User=root;Password=YOUR_PASSWORD;"
```

You don't need to create the database by hand. The migrations run when the app starts.
To apply them manually instead:

```bash
dotnet tool restore
dotnet ef database update -p src/SkillHub.Infrastructure -s src/SkillHub.Api
```

### 3. Run

```bash
dotnet restore
dotnet run --project src/SkillHub.Api     # http://localhost:5100  -> Scalar UI at /scalar/v1
dotnet run --project src/SkillHub.Web     # http://localhost:5200  -> participants web app
```

Start the API first the very first time, so it creates the database before the web app connects.
After that, either app can start on its own because both run the same idempotent initializer.

### Run everything with Docker (optional)

```bash
docker compose up --build
```

| Service | URL                              |
|---------|----------------------------------|
| API     | http://localhost:5100/scalar/v1  |
| Web     | http://localhost:5200            |
| MySQL   | localhost:3307 (root / root)     |

If port 3307 is taken, run `MYSQL_HOST_PORT=3308 docker compose up --build`. In Docker the API applies the
migrations and the seed; the web container skips that step (`Database__InitializeOnStartup=false`).

### Tests

```bash
dotnet test
```

### Database script

The final database script (structure + data) is in [`database/skillhub.sql`](database). To regenerate it:

```bash
./scripts/export-database.sh                              # from the Docker container
./scripts/export-database.sh localhost 3306 root password # from a local MySQL
```

## Credentials

The accounts come from `SkillHub_Data.xlsx`. Each row uses the password in its `Password` column. The
**instructors** are SkillHub's **employees** and can log in from the very first startup.

| Role        | Example account                | Password          |
|-------------|--------------------------------|-------------------|
| Employee    | `andres.mejia@correo.com`      | `Instructor2026*` |
| Employee    | `rosa.barrios@correo.com`      | `Instructor2026*` |
| Participant | `jorge.perez@correo.com`       | `Skillhub2026*`   |

All 6 instructors share `Instructor2026*` and all 30 participants share `Skillhub2026*`. If a row has no
password, the values of `Seed:EmployeePassword` / `Seed:ParticipantPassword` in `appsettings.json` are used.
People who sign up themselves always get the **Participant** role.

> The JWT signing key in `appsettings.json` is a development value. Replace it (for example with the
> `Jwt__Key` environment variable) before deploying.

## API

Every non-public endpoint requires `Authorization: Bearer <token>`. Get the token from `POST /api/v1/auth/login`.
In Scalar, click **Authentication** and paste the token.

| Method | Route                                      | Access                  | Description                                      |
|--------|--------------------------------------------|-------------------------|--------------------------------------------------|
| POST   | `/api/v1/auth/register`                    | Public                  | Registers a new participant                      |
| POST   | `/api/v1/auth/login`                       | Public                  | Logs in and returns the JWT                      |
| GET    | `/api/v1/workshops`                        | Public                  | Upcoming workshops with available seats          |
| GET    | `/api/v1/workshops/{id}`                   | Public                  | Workshop detail with available seats             |
| POST   | `/api/v1/workshops`                        | Employee                | Creates a workshop                               |
| GET    | `/api/v1/workshops/{id}/enrollments`       | Employee                | Enrollments of a workshop                        |
| POST   | `/api/v1/enrollments`                      | Participant             | Enrolls the authenticated participant            |
| GET    | `/api/v1/enrollments/mine`                 | Participant             | Enrollments of the authenticated participant     |
| PATCH  | `/api/v1/enrollments/{id}/cancel`          | Owner or employee       | Cancels an enrollment                            |
| POST   | `/api/v1/enrollments/import`               | Employee                | Bulk enrollment from Excel (`multipart/form-data`, field `file`) |
| GET    | `/api/v1/enrollments/{id}/certificate`     | Owner or employee       | Downloads the PDF certificate                    |
| GET    | `/api/v1/reports/top-workshops?top=10`     | Employee                | Workshops with the most active enrollments       |
| GET    | `/api/v1/instructors`                      | Employee                | Registered instructors (extra)                   |

### Request examples

```http
POST /api/v1/workshops
Authorization: Bearer <employee token>
Content-Type: application/json

{
  "code": "FOT-010",
  "title": "Night photography",
  "description": "Long exposures and light painting.",
  "instructorEmail": "<an instructor email>",
  "startsAt": "2026-12-05T18:00:00",
  "endsAt": "2026-12-05T21:00:00",
  "capacity": 15
}
```

```http
POST /api/v1/enrollments
Authorization: Bearer <participant token>
Content-Type: application/json

{ "workshopId": "<workshop id>" }
```

Bulk import response:

```json
{
  "totalRows": 70,
  "enrolledCount": 68,
  "rejectedCount": 2,
  "rejectedRows": [
    { "rowNumber": 4, "reason": "Workshop FOT-001 has no available seats." },
    { "rowNumber": 9, "reason": "Participant nobody@correo.com does not exist." }
  ]
}
```

The import accepts either a file with only the enrollment rows or the complete `SkillHub_Data.xlsx` (it uses the
`Enrollments` sheet). The first row must contain the headers `ParticipantEmail` and `WorkshopCode`.

## Business rules

- A workshop must start in the future, and it must end after it starts. Its code is unique. The instructor is given
  by email and must exist.
- The catalog only shows workshops that haven't started yet. Available seats = capacity − active enrollments.
- Enrollments start as **Active** and can become **Cancelled**.
  1. **Capacity**: only active enrollments count against the capacity.
  2. **No duplicates**: a participant can have only one active enrollment per workshop.
  3. **No schedule overlaps** with any other workshop where the participant has an active enrollment.
  4. **Only future workshops**: you can't enroll once a workshop has started.
  5. **Cancellation**: a participant can cancel up to 24 hours before the start. An employee can cancel any
     enrollment while the workshop hasn't started. Every cancellation stores who did it (user id and email) and when.
  6. **Re-enrollment**: after cancelling, a participant can enroll again if the other rules still allow it.
- **Bulk import**: each row is processed and saved on its own with the same rules. Valid rows are kept even when
  other rows fail. The response lists the rejected rows with their row number and the reason.
- **Certificate**: available only for active enrollments, to their owner or to any employee.
- All dates are in SkillHub's local time zone (`SkillHub:TimeZone`, default `America/Bogota`), because the workshops
  are in person.

## Author

- **Full name:** Juan Diego Marchena
- **Clan:** Esthercita
- **Email:** juanmarchenacomas4@gmail.com
- **ID document:** _your document number_
