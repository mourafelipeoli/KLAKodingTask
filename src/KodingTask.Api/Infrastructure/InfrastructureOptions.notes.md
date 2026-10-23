# InfrastructureOptions

Bound from `Infrastructure`. Everything the reporter can discover by itself (cluster,
service, target group, load balancer, region, task size) is discovered at run time from the
ECS task metadata and the service description. Only two things cannot be discovered and are
configured, by identifier, in the deployment's environment (never in this repository):

- `DatabaseInstanceId`: the RDS instance whose engine and version are shown;
- `CertificateArn`: the TLS certificate whose expiry date is shown.

Both are optional; without them those two facts are simply omitted.
