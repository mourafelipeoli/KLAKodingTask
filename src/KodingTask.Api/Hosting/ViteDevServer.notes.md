# ViteDevServer

Development only. Starts the web client's Vite dev server (`npm run dev`) together with the
API host and stops it when the host stops, so one `dotnet run` brings up everything.

- If something already answers at `Development:ViteServerUrl` (default
  `http://localhost:5179`), it is reused: running `npm run dev` yourself in a second
  terminal keeps working.
- Host startup waits until Vite answers (up to 90 seconds), so the first browser request
  never meets a 502 from the forwarder. If `npm` is missing or Vite crashes, startup fails
  with the reason instead of serving a half-working site.
- Vite's own output is relayed to the host log with a `[Vite]` prefix.
- On shutdown the whole process tree is killed (`npm` spawns `node`, which would otherwise
  keep the port busy).

## Why not `Microsoft.AspNetCore.SpaProxy`

The SPA proxy package does the same launching, but it intercepts requests to `/` and
redirects them to a configured URL. A redirect target cannot be `/` itself (it would loop),
and the redirect drops the query string, so a shared link such as
`/?amount=45%20100&language=fr-BE` lost its amount in development. Launching Vite from a
hosted service and forwarding every request unchanged keeps development identical to
production, where `/` serves the app directly.
