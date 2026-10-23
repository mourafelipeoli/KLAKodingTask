# InfrastructureServiceCollectionExtensions

Registers the infrastructure page services. Nothing here needs configuration to start: the
AWS clients are only created by the probe the first time the task metadata says the app runs
on AWS, so a clone running on a laptop never touches the AWS SDK credential chain.

The metadata client has a two-second timeout: the endpoint is on the same host and answers
in milliseconds, so anything slower means it is not there.
