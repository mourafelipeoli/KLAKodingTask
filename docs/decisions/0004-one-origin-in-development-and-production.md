# 4. One origin in development and production

Date: 2026-10-23 · Status: accepted

## Context

A React app is usually developed against the Vite dev server on its own port, with the API
on another port and CORS or a Vite proxy in between. Cookies, sign-in callbacks and the
content security policy then behave differently in development and in production.

## Decision

The API host serves the client in both environments. In production it serves the built
files from `wwwroot` with a fallback to `index.html`. In development it starts Vite itself
(`ViteDevServer`) and forwards every non-API request to it with YARP, so the browser only
ever talks to `https://localhost:7162`.

## Consequences

- No CORS configuration exists, because none is needed.
- One `dotnet run` starts everything.
- Hot reload works through the forwarder (Vite's websocket uses a dedicated path).

## Alternatives considered

- **`Microsoft.AspNetCore.SpaProxy`**: launches Vite the same way, but redirects requests
  for `/` to a configured address and loses the query string on the way, which broke shared
  links (`/?amount=…&language=…`) in development. Replaced by a small hosted service.
- **Separate origins with CORS**: more configuration and a development setup that differs
  from production.
