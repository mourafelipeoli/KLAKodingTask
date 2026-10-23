# InfrastructureEndpointTests

`GET /api/infrastructure` through the real pipeline.

- **Locally** (no task metadata): no AWS facts, SQLite on disk, the limits from
  configuration, zero AI calls.
- **On AWS** (fake task metadata full of ARNs, account id, cluster, service and task names,
  plus a fake probe): the facts arrive in words, and the response body contains **none** of
  those identifiers. This is the test that guards the promise of the page.
- A probe that throws still yields 200 with the hosting facts.
- Three visits ask AWS once (the snapshot is cached).
- The commit link appears only when a public repository URL is configured.
