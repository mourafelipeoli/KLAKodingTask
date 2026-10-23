# RequestRateLimits

Per-client request budgets, keyed by the client's IP address after forwarded headers are
applied (see `ForwardedHeadersSetup` for why that address cannot be spoofed behind the
load balancer).

- **Conversions**: 120 per minute per address by default (`RateLimits:ConversionsPerMinute`).
  A person typing and clicking never gets near it; a script looping over the endpoint is
  slowed to two requests per second, which keeps one client from monopolising a small
  container.
- **No queueing**: a rejected request gets 429 immediately with `Retry-After` and the same
  JSON error shape as every other refusal (`{ "error": "TooManyRequests", "message": … }`),
  so the client shows a translated message instead of hanging.

This is application hygiene, not the cost guardrail. Limits that protect money (emails,
WhatsApp messages, AI calls) live outside the code, where whoever holds the code cannot
raise them; see `docs/infrastructure.md`.
