# Configuration

Everything environment-specific is configuration. Nothing in this table is required for a
local run: the defaults in `src/KodingTask.Api/appsettings.json` start the application with
a local SQLite database and every optional feature switched off.

Settings use the standard ASP.NET Core sources: `appsettings.json`, then
`appsettings.{Environment}.json`, then user secrets (Development), then environment
variables (`Section__Key`, double underscore). Secrets never go into a JSON file in the
repository: use `dotnet user-secrets` locally and your platform's secret store when hosted.

```sh
cd src/KodingTask.Api
dotnet user-secrets set "ConnectionStrings:KodingTask" "Server=…;Database=…;User=…;Password=…"
```

## Core

| Setting | Default | Purpose |
|---|---|---|
| `Database:Provider` | `Sqlite` | `Sqlite` or `MySql`. |
| `ConnectionStrings:KodingTask` | `Data Source=App_Data/kodingtask.db` | Relative SQLite paths are anchored to the project folder. |
| `Database:MySqlServerVersion` | `8.4.0` | Declared instead of detected, so startup never depends on the database being reachable. |
| `Database:MySqlMaxRetries` | `3` | Transient-failure retries for MySQL. |
| `PublicSite:Url` | `https://klakodingtask.mourafelipe.com` | Where users are sent to try features this instance does not have. Point it at your own deployment. |
| `PublicSite:RepositoryUrl` | empty | Source code link in the footer (hidden when empty). |
| `PublicSite:AuthorName`, `PublicSite:AuthorUrl` | Felipe Moura, https://mourafelipe.com | Author link in the header and footer (hidden unless both are set). |
| `RateLimits:ConversionsPerMinute` | `120` | Per client address. |
| `App:Version` | from `versions.json` | Override of the version reported by `/health`. |
| `BUILD_COMMIT` | empty | Commit reported by `/health`; set by the deployment. |
| `Development:ViteServerUrl` | `http://localhost:5179` | Development only. |
| `Development:WebClientPath` | `../kodingtask-web` | Development only. |

## Database on MySQL

```sh
# 1. Generate the SQL (no database connection needed)
dotnet ef migrations script --idempotent \
    --project src/KodingTask.Data --startup-project src/KodingTask.Api -o kodingtask.sql

# 2. Apply it with an administrative account, then run the application with a
#    runtime account that has only SELECT, INSERT, UPDATE and DELETE on its database.
```

The application never migrates on startup.
