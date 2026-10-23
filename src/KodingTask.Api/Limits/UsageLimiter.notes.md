# UsageLimiter

Takes one unit from a daily counter if, and only if, the counter is below its limit.

The check and the increment are **one statement**:
`UPDATE DailyUsage SET Count = Count + 1 WHERE Day = @today AND Counter = @name AND Count < @limit`.
The database applies it atomically, so two requests racing for the last unit cannot both
win, even on two different containers. Reading the count and then writing it back would let
both through.

The first use of a counter on a given day has no row to update yet, so the limiter inserts
one. Two first uses can race on that insert; the loser gets a primary-key violation, clears
its change tracker and retries the conditional update, which then sees the winner's row.

Days are UTC, so the reset happens at the same moment for every visitor and every server.
A limit of zero or less never grants anything (that is how a closed limit is expressed).
