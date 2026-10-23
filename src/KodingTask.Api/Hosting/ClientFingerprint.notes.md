# ClientFingerprint

Turns a client IP address into an opaque identifier for per-client limits and logs.

- **HMAC-SHA256 with a secret key**, not a plain hash: the IPv4 space is small enough to
  reverse an unkeyed hash by brute force in minutes. Without the key, the stored value
  identifies nobody.
- The key comes from `Privacy:ClientHashKey` (a secret in the hosted deployment). Without
  it, a random key is generated per process: the per-client limits still work, they just
  reset when the process restarts. That is acceptable for a local run.
- 128 bits of the digest are kept (32 hex characters), plenty to avoid collisions between
  visitors and short enough for a counter name.
- The address is the one seen after forwarded headers are applied (see
  `ForwardedHeadersSetup`), so behind the load balancer it is the visitor's, not the
  balancer's.
