# InfrastructureFactsTests

Unit tests for the pure parts of the infrastructure page, no host and no AWS: reading
regions, cluster names and CloudWatch dimensions out of ARNs; parsing a task metadata
document shaped like the one ECS serves (including an internal container that must not
count for the start time); the hosting facts in words; and the traffic arithmetic of
`TrafficSummary` (hourly buckets, request-weighted response time, the difference between a
quiet day and an unmeasured one, CPU and memory over the last hour only).
