# ForwardedHeadersSetup

In the hosted deployment the container sits behind a load balancer that terminates TLS.
Without forwarded headers the application would believe every request came over plain
HTTP from the balancer's address: redirects and cookies would use the wrong scheme, and
per-IP rate limiting would put every visitor in the same bucket.

- **Any proxy is trusted** (`KnownIPNetworks` and `KnownProxies` cleared) because the
  balancer's addresses change and the container is only reachable from the balancer
  (network rules allow the application port from the balancer's security group only).
- **`ForwardLimit = 1`** takes only the last address of `X-Forwarded-For`, the one the
  balancer appended. A client that sends its own `X-Forwarded-For: 1.2.3.4` cannot pick its
  rate-limit bucket, because its value is further left in the list and ignored.

Run directly on a developer machine there is no proxy, the headers are absent and nothing
changes.
