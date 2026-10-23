# HealthEndpoint

`GET /health` answers `{ "status": "ok", "version", "commit", "now" }`; `HEAD /health`
answers 200 without a body; any other method gets 405.

The contract is narrow on purpose:

- **First middleware in the pipeline.** It runs before forwarded headers, security headers,
  routing, rate limiting and the SPA fallback, so nothing else can turn a health check
  into a redirect, a 429 or an `index.html`.
- **Never touches the database or any external service.** The load balancer uses this
  endpoint to decide whether the container is alive. If it checked the database, a short
  database blip would make the balancer pull every container at once and turn a partial
  outage into a total one. Dependencies are reported elsewhere (the infrastructure page).
- **Exact path, anonymous, `no-store`**, so a cache or CDN can never answer for the container.
- **Version and commit** let whoever deploys confirm that the new revision is the one
  answering, not a rollback that happened to be healthy.
