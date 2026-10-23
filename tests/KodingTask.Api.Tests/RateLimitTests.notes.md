# RateLimitTests

Each test builds its own factory with a tiny limit, so the shared fixtures of the other
test classes never run into it. The checks: the request over the limit gets 429 with
`Retry-After` and the standard JSON error, and the health endpoint is never limited (a
load balancer must always be able to reach it).
