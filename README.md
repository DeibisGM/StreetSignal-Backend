# StreetSignal API

REST API for the **StreetSignal** citizen reports platform — built with **.NET 10 (LTS)**, ASP.NET Core, Entity Framework Core, and JWT authentication.

The API faithfully implements the contract documented in [`docs/api-contracts/streetsignal-api-contracts.yml`](docs/api-contracts/streetsignal-api-contracts.yml).

## Tech stack

- **.NET 10** (LTS) · ASP.NET Core Web API · C# 14
- **EF Core 10** with **SQLite** (development) and **InMemory** (tests)
- **JWT bearer** authentication + `PasswordHasher<TUser>` from `Microsoft.AspNetCore.Identity`
- **xUnit** + **Moq** + **FluentAssertions** for unit tests
- `Microsoft.AspNetCore.Mvc.Testing` + `WebApplicationFactory<Program>` for endpoint tests
- **Swashbuckle** (Swagger UI in Development)

## Project layout

```
StreetSignal-Backend/
├── Common/                   Enums, exceptions, error codes, current-user service
├── Configuration/            JWT options, DI extension, exception middleware, validation filter
├── Controllers/              7 controllers — Health, Auth, Categories, Reports, ReportUpdates, Files, Notifications
├── Data/                     AppDbContext + DbSeeder
├── DTOs/                     Requests/ and Responses/ that match the contract
├── Mappers/                  Static extension mappers (entity ↔ DTO)
├── Models/                   6 entities — User, Category, Report, ReportUpdate, Notification, DeviceToken
├── Permissions/              Role + policy constants (StaffOnly, CitizenOnly, …)
├── Repositories/             Interfaces/ + Implementations/ (EF Core)
├── Services/                 Interfaces/ + Implementations/ (business logic)
├── UnitTests/                xUnit tests for services
├── IntegrationTests/         WebApplicationFactory<Program> endpoint tests
├── docs/api-contracts/       Source-of-truth OpenAPI YAML
├── .github/workflows/        GitHub Actions CI
├── Program.cs
├── appsettings.json / .Development.json / .Local.json
└── streetsignal-api.{csproj,slnx}
```

## Getting started

> Requires the .NET 10 SDK (`dotnet --version` should report `10.0.x`).

```bash
dotnet restore
dotnet build
dotnet run
```

The app starts on `http://localhost:5xxx` (see `Properties/launchSettings.json`). In Development the database is auto-created and seeded.

Swagger UI: `http://localhost:5xxx/swagger`

### Seeded accounts (Development only)

| Role    | Email                          | Password        |
|---------|--------------------------------|-----------------|
| Citizen | `citizen@streetsignal.test`    | `Password123!`  |
| Staff   | `staff@streetsignal.test`      | `Password123!`  |

### Seeded categories
*Accumulated garbage*, *Broken streetlight*, *Pothole*, *Water leak*, *Other*.

## Configuration

`appsettings.json` (and the optional `appsettings.Local.json` override) expose:

```json
{
  "ConnectionStrings": { "Default": "Data Source=streetsignal.db" },
  "Jwt": {
    "Issuer": "StreetSignal",
    "Audience": "StreetSignal",
    "SigningKey": "replace-this-with-a-strong-32+-char-secret-only-for-dev",
    "ExpiresInSeconds": 86400
  }
}
```

**Production:** override `Jwt:SigningKey` and `ConnectionStrings:Default` via environment variables or your secret manager.

## Tests

```bash
dotnet test
```

- **UnitTests/** — fast, mock-based tests for every service (`AuthService`, `CategoryService`, `ReportService`, `ReportUpdateService`, `NotificationService`, `JwtTokenService`).
- **IntegrationTests/** — end-to-end tests of every endpoint group via `WebApplicationFactory<Program>` with the EF Core InMemory provider. Cover happy paths and contract-defined error codes (`VALIDATION_ERROR`, `EMAIL_ALREADY_EXISTS`, `INVALID_CREDENTIALS`, `REPORT_NOT_EDITABLE`, `UNAUTHORIZED`, `FORBIDDEN`, `NOT_FOUND`).

## Endpoint summary

| Method | Path                                       | Auth                | Contract behaviour                                          |
|--------|--------------------------------------------|---------------------|-------------------------------------------------------------|
| GET    | `/api/health`                              | Anonymous           | Health probe                                                |
| POST   | `/api/auth/register`                       | Anonymous           | Create Citizen; `409 EMAIL_ALREADY_EXISTS` on duplicate     |
| POST   | `/api/auth/login`                          | Anonymous           | `401 INVALID_CREDENTIALS` on failure                        |
| GET    | `/api/auth/me`                             | Authenticated       | Current profile                                             |
| POST   | `/api/auth/logout`                         | Authenticated       | `204`                                                       |
| GET    | `/api/categories`                          | Anonymous (staff to include inactive) | Returns active categories; inactives only for staff |
| GET    | `/api/reports`                             | Staff               | Paginated list with filters                                 |
| POST   | `/api/reports`                             | Authenticated       | `201` with `Location` header                                |
| GET    | `/api/reports/my`                          | Authenticated       | Caller's own reports                                        |
| GET    | `/api/reports/{id}`                        | Authenticated       | Citizens only see their own; staff see all                  |
| PATCH  | `/api/reports/{id}`                        | Citizen owner       | Only `Pending`; otherwise `409 REPORT_NOT_EDITABLE`         |
| PATCH  | `/api/reports/{id}/status`                 | Staff               | Adds `StatusChange` update + notification                   |
| GET    | `/api/reports/{id}/updates`                | Authenticated       | Owner or staff                                              |
| POST   | `/api/reports/{id}/updates`                | Authenticated       | Staff comments notify the citizen                           |
| POST   | `/api/files/upload`                        | Authenticated       | jpg/jpeg/png/webp; `413 FILE_TOO_LARGE` over 5 MB           |
| POST   | `/api/notifications/device-token`          | Authenticated       | Upserts (token, platform) for the user                      |
| GET    | `/api/notifications`                       | Authenticated       | Caller's notifications                                      |
| PATCH  | `/api/notifications/{id}/read`             | Authenticated       | Owner only                                                  |

## Error contract

All non-validation errors use:

```json
{ "code": "ERROR_CODE", "message": "Human readable message." }
```

Validation errors (400) use the extended shape:

```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred.",
  "errors": { "fieldName": ["Reason 1", "Reason 2"] }
}
```

## Continuous Integration

The GitHub Actions workflow at `.github/workflows/dotnet.yml` runs on pushes to `main`, `develop`, and `feature/**` branches, and on every Pull Request to `main` / `develop`. It restores, builds in Release, runs all tests, and uploads `*.trx` results as artifacts.
