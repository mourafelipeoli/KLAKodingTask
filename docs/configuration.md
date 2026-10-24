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
| `PublicSite:AuthorName`, `PublicSite:AuthorUrl` | Felipe Moura, https://mourafelipe.com | Author links in the top bar and the footer (hidden unless both are set). |
| `PublicSite:AuthorTerminalUrl` | https://mourafelipe.com/terminal | Second link in the top bar (hidden when empty). |
| `RateLimits:ConversionsPerMinute` | `120` | Per client address. |
| `RateLimits:AiPerMinute` | `10` | Per client address, for the AI engine. |
| `RateLimits:InfrastructurePerMinute` | `30` | Per client address, for the infrastructure page. |
| `RateLimits:AccountPerMinute` | `20` | Per client address, for every account, sign-in and delivery call. |
| `RateLimits:TranslationsPerMinute` | `30` | Per client address, for the texts pages report to the translation system. |
| `Privacy:ClientHashKey` | empty (random per process) | Secret key of the anonymised client fingerprints used by per-client limits. Store it as a secret. |
| `App:Version` | from `versions.json` | Override of the version reported by `/health`. |
| `BUILD_COMMIT` | empty | Commit reported by `/health`; set by the deployment. |
| `BUILD_DATE` | empty | Build time shown on the infrastructure page; set by the deployment. |
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
| `EmailsPerDay` | `100` | Every email of the instance per UTC day (results). |
| `WhatsAppPerDay` | `50` | Every WhatsApp message per UTC day (results). |
| `DeliveriesPerUserPerDay` | `10` | Results one account may send to itself per UTC day, both channels together. |
| `TranslationCharactersPerDay` | `50000` | Characters sent to DeepL per UTC day. A whole new language takes about 7,200. |
| `TermRegistrationsPerDay` | `200` | New texts pages may report per UTC day, all visitors together. |
| `TermRegistrationsPerClientPerDay` | `20` | New texts one client address may report per UTC day. |

A cap missing from the parameter store reads as zero, which switches its feature off.

## Email (Amazon SES)

On when `Email:Sender` and `Email:Region` are set. Results only go to signed-in visitors, so
email is useful once sign-in is configured too.

| Setting | Default | Purpose |
|---|---|---|
| `Email:Sender` | empty | The `From` address. Must be a verified SES identity (the address or its domain). |
| `Email:SenderName` | `Koding Task` | Display name next to it. |
| `Email:Region` | empty | SES region of that identity. |
| `Email:ConfigurationSet` | empty | Optional SES configuration set, to get sending metrics in CloudWatch. |
| `Email:Mode` | `Ses` | `Log` writes emails to the application log instead (Development only; refuses to start elsewhere). |

Minimal IAM permission: `ses:SendEmail` on the identity and the configuration set, with the
condition `ses:FromAddress` equal to the sender, so the role cannot send as anyone else.
See `deploy/aws/task-role-policy.template.json`.

## WhatsApp (Meta Cloud API)

On when the phone number id, the access token and the result template are set.

| Setting | Default | Purpose |
|---|---|---|
| `WhatsApp:PhoneNumberId` | empty | The sending number's id in the WhatsApp Business account. |
| `WhatsApp:AccessToken` | empty | System-user token with `whatsapp_business_messaging`. A secret: use a secret store. |
| `WhatsApp:ResultTemplate` | `koding_task_result` | UTILITY template, body `{{1}}` = amount, `{{2}}` = words. |
| `WhatsApp:FallbackLanguage` | `en_US` | Used when a template is not approved in the visitor's language. |
| `WhatsApp:ApiBaseUrl` | `https://graph.facebook.com/v22.0` | Graph API version. |
| `WhatsApp:TimeoutSeconds` | `15` | Per request; no automatic retry (a duplicate message is worse than a failed one). |
| `WhatsApp:Mode` | `Meta` | `Log` writes messages to the application log instead (Development only). |

Business-initiated WhatsApp messages can only be approved templates, so the application can
never send free text: it only fills in the amount and the words. Results go only to the
number the identity provider verified for the account (claims `phone_number` and
`phone_number_verified`); the number is managed on the provider, never in this application.
A send needs a check of the number less than ten minutes old: the web client gets one with
a silent round trip to the provider (`prompt=none`) and sends on the way back.

### Trying deliveries locally

Deliveries need a signed-in visitor, and signing in needs an OpenID Connect provider (next
section). With one configured, the senders can write to the console instead of sending:

```sh
cd src/KodingTask.Api
dotnet user-secrets set "Email:Mode" "Log"
dotnet user-secrets set "Email:Sender" "dev@example.com"
dotnet user-secrets set "WhatsApp:Mode" "Log"
```

Sign in and send results. Without a provider,
the converter works as always and the send buttons continue on the deployed version.

## Translations (DeepL)

The interface languages are the members of `InterfaceLanguage`
(`src/KodingTask.Api/Translations/InterfaceLanguage.cs`); see
[decision 11](decisions/0011-a-new-language-is-one-enum-value.md). Everything works without
a key: the bundled dictionaries are imported on start, pages report texts they do not know,
and the review page lists them. With a key, a background batch machine-translates every
text missing in a language.

| Setting | Default | Purpose |
|---|---|---|
| `DeepL:ApiKey` | empty | DeepL API key. A secret. Free keys end in `:fx` and use the free endpoint. |
| `DeepL:BaseUrl` | from the key | Only to force an endpoint. |
| `DeepL:BatchSize` | `50` | Texts per request (DeepL's maximum). |
| `DeepL:IntervalSeconds` | `120` | How often the batch looks for missing texts when nothing wakes it. |
| `DeepL:TimeoutSeconds` | `20` | Per request. |
| `DeepL:RetryDelaysSeconds` | `[1, 3, 8, 20]` | Waits after 429 or 5xx when DeepL sends no `Retry-After`. |
| `Translations:SeedOnStartup` | `true` | Imports the bundled dictionaries on start. |
| `Admin:Emails` | `[]` | Addresses (verified by the identity provider) that may open `/admin/translations`. Locally: `dotnet user-secrets set "Admin:Emails:0" "you@example.com"`. |

The daily character cap and the registration caps are in [Limits](#limits).

## Sign-in with fmID (OpenID Connect)

The only way to sign in; there are no passwords on this side. On when all three settings
are set; without them there are no accounts. fmID is the author's own OpenID Connect
provider; any provider that issues `sub`, `email` and `email_verified` in the id_token works
the same way. Creating an account, confirming the address and resetting the password happen
on the provider's pages.

| Setting | Default | Purpose |
|---|---|---|
| `FmId:Authority` | empty | The provider's issuer URL (with its trailing slash). The discovery document is read from it. |
| `FmId:ClientId` | empty | The client registered at the provider for this instance. |
| `FmId:ClientSecret` | empty | That client's secret. A secret: user-secrets locally, a secret store when hosted. |
| `FmId:AccountUrl` | empty | Optional: the provider's account page, where people add and confirm their WhatsApp number. The send panel links to it. |

Register the client at the provider with:

- redirect URI `https://<host>/signin-fmid` and post-logout redirect URI
  `https://<host>/signout-fmid` (matched exactly, port included);
- authorization code with PKCE (S256), query response mode, client secret at the token
  endpoint;
- scopes `openid profile email phone` (`phone` for the WhatsApp number).

The provider's origin is added to the `form-action` directive of the content security
policy automatically. The first sign-in links the provider's subject to the local account
with the same verified address, or creates one.

To try it locally against a provider that has a development client for
`https://localhost:7162`:

```sh
cd src/KodingTask.Api
dotnet user-secrets set "FmId:Authority" "https://<provider>/"
dotnet user-secrets set "FmId:ClientId" "<development client id>"
dotnet user-secrets set "FmId:ClientSecret" "<development client secret>"
```

## Infrastructure page

The page at `/infrastructure` works without any setting. Off AWS it says that the app runs
locally. On Amazon ECS it discovers the cluster, service, target group and load balancer by
itself (task metadata, then the service description); only two things must be named:

| Setting | Default | Purpose |
|---|---|---|
| `Infrastructure:CertificateArn` | empty | ACM certificate whose expiry date is shown. |
| `Infrastructure:DatabaseInstanceId` | empty | RDS instance whose engine and version are shown (with `Database:Provider=MySql`). |
| `Infrastructure:CacheMinutes` | `5` | How long the AWS facts are cached. |
| `Infrastructure:AwsCallTimeoutSeconds` | `5` | Per AWS call; a slow or failing call only hides its own facts. |

Read-only permissions for the task role, all optional (a missing one hides its facts and
logs the AWS error code): `ecs:DescribeServices` on the service,
`elasticloadbalancing:DescribeTargetHealth` and `elasticloadbalancing:DescribeTargetGroups`,
`cloudwatch:GetMetricData`, `rds:DescribeDBInstances` on the instance and
`acm:DescribeCertificate` on the certificate. See
`deploy/aws/task-role-policy.template.json` and
[decision 5](decisions/0005-infrastructure-page-from-read-only-facts.md).

## Database on MySQL

```sh
# 1. Generate the SQL (no database connection needed)
dotnet ef migrations script --idempotent \
    --project src/KodingTask.Data --startup-project src/KodingTask.Api -o kodingtask.sql

# 2. Apply it with an administrative account, then run the application with a
#    runtime account that has only SELECT, INSERT, UPDATE and DELETE on its database.
```

The application never migrates on startup.
