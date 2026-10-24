# ApiFactory

Starts the real `Program` in memory (`WebApplicationFactory`), with the whole pipeline:
health endpoint, forwarded headers, security headers, rate limiting, controllers, the SPA
fallback.

- Environment `Testing`, so production-only behaviour (content security policy, HSTS, the
  `index.html` fallback instead of the Vite forwarder) is what gets tested.
- Each factory gets its own SQLite file in the temp folder, deleted on dispose. No test
  touches a real database or any external service.
- `Settings` overrides configuration per factory (the rate-limit test lowers the limit).
  `UseSetting` is used instead of `ConfigureAppConfiguration` because the minimal hosting
  model reads configuration while `Program` runs, before later configuration sources are
  applied.
- `ServiceOverrides` replaces services after `Program` registered them (the infrastructure
  tests swap the ECS task metadata and the AWS probe for fakes to pretend to run on AWS).

- `Translations:SeedOnStartup` is off: importing the bundled dictionaries on start would
  race with the tests' own database changes. Tests that need them import explicitly.
