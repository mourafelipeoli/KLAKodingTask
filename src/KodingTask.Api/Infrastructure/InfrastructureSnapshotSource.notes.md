# InfrastructureSnapshotSource

Holds the slow, rarely changing part of the infrastructure page: where the task runs (task
metadata) and what AWS says about it (service, targets, metrics, certificate, database).
Taken at most once every `Infrastructure:CacheMinutes` (5 by default) for the whole process,
so however many people open the page, AWS sees about a dozen read calls per five minutes.

- One refresh at a time (a semaphore), the others wait and reuse its result.
- The refresh itself ignores the request cancellation: a visitor closing the tab must not
  waste a half-finished snapshot. Every AWS call has its own timeout instead.
- A failing probe never fails the page: the snapshot then holds only the hosting facts.
- Off AWS there is no metadata, so no AWS call is made at all; the start time is then the
  start of this process.
