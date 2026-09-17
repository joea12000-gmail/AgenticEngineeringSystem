## Implementation notes

- Persistence: EF Core + SQLite in-memory using a shared open connection (SqliteConnectionHolder). Data is ephemeral across restarts.
- Containerization: Dockerfile included (multi-stage). The in-memory DB will be ephemeral in a container; for persistent demo use a file-backed SQLite or mount a volume.
- Tests: Separate projects for unit tests and integration tests. Integration tests use real Sqlite in-memory connection holder to exercise EF Core behavior.
