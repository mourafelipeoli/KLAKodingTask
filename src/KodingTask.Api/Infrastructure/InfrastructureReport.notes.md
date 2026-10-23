# InfrastructureReport

What `GET /api/infrastructure` returns: a picture of where and how this instance runs, for
the "Where this runs" page.

## What is deliberately absent

No account id, ARN, resource name (cluster, service, load balancer, database instance,
target group), IP address, host name, task or instance id, security-group id, endpoint or
error message. Every field is either a plain fact a visitor could learn from the outside
anyway (it runs on AWS in Ohio, over HTTPS) or an aggregate (copies running, requests in
the last day, average response time, how much of a cap is used). Each record is built field
by field in `InfrastructureReporter`; no AWS response object is ever serialised.

## Nullable means "unknown here"

On a developer machine there is no AWS: `Hosting.OnAws` is false and the AWS-derived parts
(`Service`, `Traffic`, region, certificate) are null. Hosted, a single AWS call that fails or
times out leaves only its own fields null; the page shows "not available" for those and
the rest normally.
