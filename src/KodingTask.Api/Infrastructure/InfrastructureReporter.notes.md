# InfrastructureReporter

Composes the `InfrastructureReport` for one request. Scoped, because the live part reads the
database:

| Part | Source | Freshness |
|---|---|---|
| Hosting, service, traffic, certificate, managed database | `InfrastructureSnapshotSource` | cached, minutes |
| Build | configuration (`versions.json`, `BUILD_COMMIT`, `BUILD_DATE`) | fixed per deploy |
| Data | database options + snapshot | fixed per deploy |
| Guardrails | limits (Parameter Store or configuration), `DailyUsage`, `AiCalls` | live |
| Features | `FeatureAvailability` | fixed per start |

The guardrail numbers are the same ones the AI engine enforces (same counter name, same
month-to-date query in `AiCallQueries`), so the page can never show a different picture
from the one the guard acts on. When the limits cannot be read they are closed, and the page
says the paid features are paused, which is exactly what visitors would experience.

The commit link is only built when a public repository URL is configured.
