# 5. The infrastructure page shows read-only facts, never identifiers

Date: 2026-10-23 · Status: accepted

## Context

The hosted version has a page that shows, for a non-technical visitor, where and how the
application runs: region, size of the container, copies running, traffic, database,
certificate, and how close the paid features are to their limits. The repository is
public and the page is public, so neither may reveal anything that helps someone target
the account: account ids, ARNs, resource names, addresses, endpoints.

## Decision

- The server builds a whitelisted report, field by field (`InfrastructureReporter`); no AWS
  response object is ever serialised. Facts are either things a visitor could see from the
  outside anyway (it runs in Ohio, over HTTPS) or aggregates (copies, requests per hour,
  average response time, CPU, share of a limit used).
- Where the task runs comes from the ECS task metadata endpoint, which needs no permission.
  Everything else comes from a handful of read-only AWS calls (`DescribeServices`,
  `DescribeTargetHealth`, `DescribeTargetGroups`, `GetMetricData`, `DescribeDBInstances`,
  `DescribeCertificate`), each with its own timeout and each optional.
- The AWS part is cached for five minutes for the whole process; the guardrail numbers are
  read live from the same counters the AI engine enforces.
- A test feeds the endpoint fake metadata full of ARNs and names and asserts that none of
  them appears in the response.

## Consequences

- The task role gains read-only permissions. Two of them (`elasticloadbalancing:Describe*`,
  `cloudwatch:GetMetricData`) cannot be scoped to a resource; they reveal only health and
  numbers.
- `ecs:DescribeTaskDefinition` is deliberately not used: it cannot be scoped either, and it
  would let the task read the environment variables of every task definition in the account.
- On a developer machine the page says that the app runs locally and links to the hosted
  version; nothing calls AWS.

## Alternatives considered

- **A static page describing the architecture**: no risk, but no evidence; it could drift
  from what is really deployed.
- **Reading facts in the browser from public endpoints**: impossible without exposing
  credentials or identifiers.
