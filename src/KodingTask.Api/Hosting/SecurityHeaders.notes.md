# SecurityHeaders

Response headers that cost nothing and close common attack paths.

- **Content Security Policy** (outside Development): scripts, fonts, connections and
  images only from the site itself. The single-page application is built by Vite into
  static files, so it needs no inline script; the anti-flash theme script is an external
  file (`/theme-init.js`) for exactly that reason. Styles allow `'unsafe-inline'` because
  React style attributes count as inline styles; style injection is far less dangerous than
  script injection, and scripts stay locked down.
- **No CSP in Development**: the Vite dev server injects inline scripts and connects to its
  own websocket for hot reload.
- **`form-action`** lists the origins a form may submit to. Sign-in with an external OpenID
  Connect provider starts with a form POST to this site that answers with a redirect to the
  provider, and browsers apply `form-action` to that redirect too. The provider's origin is
  therefore passed in by `Program` once external login is configured; without it the
  browser would block the sign-in silently.
- `frame-ancestors 'none'` and `X-Frame-Options: DENY` prevent click-jacking;
  `nosniff`, a strict referrer policy, a closed permissions policy and
  `Cross-Origin-Opener-Policy` complete the set.

Headers are added in `OnStarting`, so they also reach responses produced by later
middleware (static files, the SPA fallback, error pages).
