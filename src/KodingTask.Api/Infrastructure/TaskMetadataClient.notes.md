# TaskMetadataClient

Reads the ECS task metadata endpoint (version 4), which the container agent exposes to every
task at the address in `ECS_CONTAINER_METADATA_URI_V4`. It needs **no IAM permission** and
never leaves the host, which makes it the cheapest and safest source for: region (from the
task ARN), cluster and service names, availability zone, launch type, task size (vCPU and
memory) and start time.

Only the fields the page uses are deserialised. The cluster, service and task ARN are read
to derive the region and to call `DescribeServices`, and are never returned to the client.

The start time is the earliest `StartedAt` of the task's own containers (type `NORMAL`);
internal containers the agent adds are ignored. Before any container reports one, the end
of the image pull stands in.

Outside ECS the variable does not exist and the client returns `null` without a request.
A failure (agent unavailable, malformed JSON, timeout) is logged as a warning and treated
as "no metadata". `ITaskMetadataSource` lets the tests pretend to run on AWS.
