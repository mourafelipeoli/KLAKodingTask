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
| `RateLimits:AiPerMinute` | `10` | Per client address, for the AI engine. |
| `Privacy:ClientHashKey` | empty (random per process) | Secret key of the anonymised client fingerprints used by per-client limits. Store it as a secret. |
| `App:Version` | from `versions.json` | Override of the version reported by `/health`. |
| `BUILD_COMMIT` | empty | Commit reported by `/health`; set by the deployment. |
| `Development:ViteServerUrl` | `http://localhost:5179` | Development only. |
| `Development:WebClientPath` | `../kodingtask-web` | Development only. |

## AI engine (Amazon Bedrock)

Off unless both `Ai:ModelId` and `Ai:Region` are set. Credentials come from the default AWS
chain: an IAM role when hosted, a named profile locally (`AWS_PROFILE=… dotnet run …`).

| Setting | Default | Purpose |
|---|---|---|
| `Ai:ModelId` | empty | A Bedrock model or inference profile id, e.g. `us.anthropic.claude-haiku-5-5`, or the ARN of an application inference profile. |
| `Ai:Region` | empty | Bedrock region, e.g. `us-east-2`. |
| `Ai:ModelLabel` | Claude Haiku 5.5 on Amazon Bedrock | Shown next to the AI answer. |
| `Ai:MaxOutputTokens` | `2000` | Includes the model's thinking. |
| `Ai:Effort` | empty (model default) | Optional effort level sent to the model. |
| `Ai:MaxConcurrency` | `2` | Simultaneous model calls per instance. |
| `Ai:TimeoutSeconds` | `30` | Per model request. |
| `Ai:Pricing:*PerMillionUsd` | Haiku 5.5 list prices | Used to estimate spend for the monthly budget. |

Minimal IAM permissions for the role that runs the application: `bedrock:InvokeModel` on the
inference profile (and, for a cross-region profile, on the foundation model in each region
it routes to). See `deploy/aws/task-role-policy.template.json`.

## Limits

Caps on paid operations. Without `Limits:SsmPath` they are read from this section; with it,
from AWS Systems Manager Parameter Store, one parameter per name under that path (for example
`/kodingtask/limits/AiCallsPerDay`), and this section is ignored. Keeping them in the
parameter store means the people who can change the code cannot change the caps. If the
store cannot be read, every paid feature is closed until it can.

| Setting / parameter name | Default | Purpose |
|---|---|---|
| `Limits:SsmPath` | empty | Parameter Store path, e.g. `/kodingtask/limits/`. |
| `Limits:SsmRegion` | the SDK's region | Only if the parameters live in another region. |
| `Limits:CacheSeconds` | `60` | How long a value is cached (and how quickly a change applies). |
| `KillSwitch` | `false` | `true` closes every paid feature at once. |
| `AiCallsPerDay` | `200` | All visitors together, per UTC day. |
| `AiCallsPerClientPerDay` | `20` | Per client address, per UTC day. |
| `AiMonthlyBudgetUsd` | `2` | Estimated AI spend per calendar month (UTC). |

## Database on MySQL

```sh
# 1. Generate the SQL (no database connection needed)
dotnet ef migrations script --idempotent \
    --project src/KodingTask.Data --startup-project src/KodingTask.Api -o kodingtask.sql

# 2. Apply it with an administrative account, then run the application with a
#    runtime account that has only SELECT, INSERT, UPDATE and DELETE on its database.
```

The application never migrates on startup.
