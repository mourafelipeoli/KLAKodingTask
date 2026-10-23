# DatabaseSetup

Registers the database and, for SQLite only, prepares it on startup.

## SQLite: zero setup for a fresh clone

- A relative `Data Source` is anchored to the project's content root
  (`src/KodingTask.Api/App_Data/kodingtask.db`), so the file lands in the same place
  whatever directory `dotnet run` was started from. The folder is git-ignored.
- The schema is created with `EnsureCreated()`. SQLite never runs migrations: the
  migrations in this repository are written for MySQL.
- **Stale schema detection.** `EnsureCreated()` does nothing when the file already exists,
  so after a model change an old local database would fail at the first query. A SHA-256
  of the generated create script is stored next to the database file; when it differs,
  the local database is dropped and recreated, with a warning in the log. Local data is
  disposable by definition (keys, test accounts), so a rebuild is the right trade-off.

## MySQL: nothing happens on startup

The hosted database is migrated by an operator, with an administrative account, before
the deploy that needs the change. The runtime account has no DDL rights, so an automatic
migration would fail anyway, and a container restart must never be able to change the
schema of a shared database server.
