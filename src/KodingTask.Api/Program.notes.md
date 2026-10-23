# Program

The composition root: services first, then the request pipeline. The order of the
pipeline is the part that matters.

1. **`/health`** answers before anything else can redirect, rate-limit or rewrite it.
2. **Forwarded headers**, so everything after sees the real client address and scheme.
3. **Security headers** on every response, including static files and errors.
4. **HSTS** outside Development (it is only emitted on HTTPS requests, which the forwarded
   `X-Forwarded-Proto` makes visible behind the load balancer).
5. **Database preparation**: creates the local SQLite file on first run; does nothing for
   MySQL (see `DatabaseSetup`).
6. **Static files** of the built single-page app (production only; the folder is empty in
   development).
7. **Routing and rate limiting**, then the API controllers and the OpenAPI document
   (`/openapi/v1.json`, kept public because the API is the deliverable).
8. **Unknown `/api/...` paths answer 404 JSON.** Without this, the SPA fallback below would
   answer `index.html` with 200 to a mistyped API call, which is confusing to debug.
9. **The single-page app**: in development every other path is forwarded to the Vite dev
   server, so the browser only ever talks to `https://localhost:7162` (same origin as
   production: cookies, callbacks and CSP behave the same). In production unknown paths
   fall back to `index.html` and the client-side router takes over.

## Development startup

In Development, `ViteDevServer` starts `npm run dev` with the host and waits until Vite
answers, so one `dotnet run` serves the whole application at `https://localhost:7162`.
The first build also installs the web client's packages (see the
`EnsureWebClientDependencies` target in the project file).

`public partial class Program` exposes the entry point to `WebApplicationFactory` in the
API tests.
