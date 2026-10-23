# KodingTaskDbContextOptions

The one place that knows how each provider is wired, shared by the application host and
the tests.

For MySQL, `EnableRetryOnFailure` absorbs the short connection drops a managed database
produces during maintenance or failover. It also means user code must not open its own
transactions without the execution strategy; when a feature needs an explicit transaction
it has to wrap it in `Database.CreateExecutionStrategy().ExecuteAsync(...)`.
