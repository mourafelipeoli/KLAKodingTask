# KodingTaskDbContextDesignTimeFactory

Used by `dotnet ef` only. It builds a MySQL context with a placeholder connection string and
a declared server version, so migrations are generated **without connecting to any
database** and without reading the application's configuration or secrets.

```sh
dotnet ef migrations add <Name> --project src/KodingTask.Data --startup-project src/KodingTask.Api
dotnet ef migrations script --idempotent --project src/KodingTask.Data --startup-project src/KodingTask.Api -o migration.sql
```

Applying a migration is a deliberate operator step against the real database, with an
administrative account; the application never migrates on startup.
