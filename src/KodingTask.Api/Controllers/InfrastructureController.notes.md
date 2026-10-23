# InfrastructureController

`GET /api/infrastructure`: the facts behind the "Where this runs" page. Public and read-only;
its own rate limit (`RateLimits:InfrastructurePerMinute`, 30 per minute per client) keeps it
from being used to hammer the database, and the AWS part is cached for minutes anyway.

`Https` is the scheme as the client saw it: behind the load balancer it comes from the
forwarded headers, so it is `true` in production and `false` on `http://localhost`.
