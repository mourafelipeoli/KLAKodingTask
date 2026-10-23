# DailyUsage

A counter per UTC day and name (`ai:calls`, `ai:client:<hash>`, `email:sent`, …), with
`(Day, Counter)` as the primary key.

The row exists so that a limit can be consumed **atomically** in the database: one
`UPDATE … SET Count = Count + 1 WHERE Day = … AND Counter = … AND Count < limit` either
takes a unit or touches no row. That holds across several containers and across restarts,
which an in-memory counter cannot do. See `UsageLimiter` in the API.

Old days are deleted by the retention job; the table stays small.
