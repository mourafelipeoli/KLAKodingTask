# RetentionJob

Deletes what the application no longer needs, once a day (the first run a minute after
start):

| Table | Kept for |
|---|---|
| `OutboundMessages` | 90 days |
| `AiCalls` | 90 days (the monthly AI budget only needs the current month) |
| `ConversionRecords` | 90 days (and never more than ten per person) |
| `DailyUsage` | 40 days (caps only read today's row) |
| `PhoneChallenges` | one day after expiry |

This keeps the database from growing without limit even if nobody ever looks at it, which
is part of "the code holder cannot grow the database": the runtime database user has
`DELETE` on its own tables and nothing more. A failed run is logged and retried the next day.
