# AwsInfrastructureProbe

The read-only AWS calls behind the infrastructure page. Each one is optional and fail-soft:
it has its own timeout (`Infrastructure:AwsCallTimeoutSeconds`), and an error (most often a
missing permission) is logged with the AWS error code only and leaves its facts null.

| Fact | Call | IAM action and resource |
|---|---|---|
| Copies running and desired, rollout, last deploy, rollback, public IP | `DescribeServices` | `ecs:DescribeServices` on this service |
| Healthy and registered targets | `DescribeTargetHealth` | `elasticloadbalancing:DescribeTargetHealth` (`*`) |
| Load balancer of the target group, for the metrics | `DescribeTargetGroups` | `elasticloadbalancing:DescribeTargetGroups` (`*`) |
| Requests, response time, CPU, memory | `GetMetricData` | `cloudwatch:GetMetricData` (`*`) |
| Database engine, version, Multi-AZ | `DescribeDBInstances` | `rds:DescribeDBInstances` on this instance |
| Certificate expiry | `DescribeCertificate` | `acm:DescribeCertificate` on this certificate |

The Elastic Load Balancing describes and `GetMetricData` do not support resource-level
permissions, hence `*`; they are read-only and reveal nothing beyond health and numbers.
Deliberately **not** used: `ecs:DescribeTaskDefinition`, which also needs `*` and would let
the task read the environment variables of every task definition in the account.

The cluster and service names come from the task metadata; the target group from the
service; the load balancer from the target group. Only the database instance and the
certificate must be configured (see `InfrastructureOptions`).

The five SDK clients are created together the first time a region is known and reused;
they retry once at most, because a slow answer is worth less than a fast "not available".
