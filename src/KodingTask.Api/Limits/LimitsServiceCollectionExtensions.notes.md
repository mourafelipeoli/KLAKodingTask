# LimitsServiceCollectionExtensions

Registers the limit source and the limiter. The AWS Systems Manager client is registered
only when `Limits:SsmPath` is set, so a local run never needs AWS credentials. Without it,
the container fills the source's optional SSM parameter with its default (`null`) and the
source reads the `Limits` section instead.

The SSM region defaults to the SDK's own resolution (in ECS, the task's region); set
`Limits:SsmRegion` only when the parameters live elsewhere.
