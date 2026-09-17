# Agentic Engineering System POC

This repository implements the proof of concept described in `Agentic_Engineering_System_POC.html`.

The current build is a .NET 10 solution with:

- ASP.NET Core minimal API host.
- URL shortener reference workload.
- EF Core persistence using SQLite in-memory storage.
- Initial orchestration and governance domain models.
- Deterministic policy engine for approval guardrails.
- MSTest coverage for the first service and policy slices.

## Projects

- `src/AgenticEngineeringSystem.Api` - HTTP API, endpoint routing, health check, OpenAPI in development.
- `src/AgenticEngineeringSystem.Core` - domain entities, contracts, orchestration models, governance policies.
- `src/AgenticEngineeringSystem.Infrastructure` - EF Core DbContext, SQLite in-memory configuration, service implementations.
- `tests/AgenticEngineeringSystem.Tests` - focused unit/integration-style tests against SQLite in-memory.

## Run

```powershell
dotnet build AgenticEngineeringSystem.slnx
dotnet test AgenticEngineeringSystem.slnx
dotnet run --project .\src\AgenticEngineeringSystem.Api\AgenticEngineeringSystem.Api.csproj
```

Useful endpoints:

- `GET /health`
- `POST /api/urls`
- `GET /{shortCode}`
- `GET /api/urls/{shortCode}/analytics`
- `DELETE /api/urls/{shortCode}`
- `GET /api/governance/policy?action=deploy&target=production`
- `GET /api/workflows`

Example URL creation payload:

```json
{
  "originalUrl": "https://example.com/articles/1",
  "customCode": "example-1"
}
```

## Persistence

The POC uses EF Core with an open shared SQLite in-memory connection. Data is relational and queryable while remaining local and disposable for demonstration runs. A production version would replace this with a durable SQLite, SQL Server, or PostgreSQL provider and add migrations.
