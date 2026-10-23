# AwsArns

Pieces of AWS resource names that the reporter needs **internally** to make its next call:

- the region, from the task ARN (`arn:aws:ecs:<region>:...`);
- the cluster name, from the metadata `Cluster` field (an ARN or already a short name);
- the CloudWatch dimension values of a load balancer (`app/<name>/<id>`) and a target group
  (`targetgroup/<name>/<id>`), which are the tails of their ARNs.

None of these values is ever returned to the client; see `InfrastructureReport.notes.md`.
