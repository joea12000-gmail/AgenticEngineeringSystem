# Repository Instructions

## Scope

These instructions apply to the whole repository.

## Engineering Defaults

- Target .NET 10 for all projects.
- Keep domain contracts and entities in `AgenticEngineeringSystem.Core`.
- Keep EF Core and other infrastructure details in `AgenticEngineeringSystem.Infrastructure`.
- Keep HTTP endpoint wiring in `AgenticEngineeringSystem.Api`.
- Use EF Core with SQLite in-memory storage for the POC unless the user explicitly changes persistence requirements.
- Prefer deterministic implementations for agent/model behavior so orchestration, policy, retries, and failure paths can be tested repeatably.

## Verification

Run these before handing off code changes when feasible:

```powershell
dotnet build AgenticEngineeringSystem.slnx
dotnet test AgenticEngineeringSystem.slnx
```

## Documentation

- Update `WORK_LOG.md` after meaningful changes so future sessions can resume quickly.
- Keep `README.md` aligned with runnable endpoints and project structure.
- Record larger design choices in `docs/decisions/` when the implementation moves beyond the initial POC slice.
