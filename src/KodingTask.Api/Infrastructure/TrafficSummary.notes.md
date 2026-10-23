# TrafficSummary

Turns raw CloudWatch data points into the four traffic figures of the page. Pure and
deterministic, so it is unit-tested without AWS.

- **Requests per hour**: 24 buckets, the oldest first; the last one is the current, still
  running hour. The load balancer metric `RequestCount` (sum per hour) is placed by
  timestamp, so hours without any request (CloudWatch returns no point for them) stay at zero.
- **Average response time**: the load balancer reports an hourly *average*
  `TargetResponseTime`; the average of the day is the mean of those hourly averages
  **weighted by the requests of each hour**, so a quiet hour with one slow request does not
  dominate. Shown in milliseconds.
- **CPU and memory**: the mean of the five-minute averages of the ECS service in the last hour.

`null` means "not measured" (that query did not run or failed); zero means "measured, none".
A day without requests therefore shows 0 requests and no average response time.
