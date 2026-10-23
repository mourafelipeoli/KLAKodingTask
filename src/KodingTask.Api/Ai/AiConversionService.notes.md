# AiConversionService

Asks the AI model for the words of an amount that the deterministic engine has already
converted, and reports whether the two agree.

## The order of the guards

Every check that can refuse runs **before** the model is called, cheapest first:

1. not configured → `Unavailable` (a fresh clone);
2. kill switch → `Paused`;
3. month-to-date spend (sum of `AiCalls.CostMicroUsd`) at or above the monthly budget →
   `MonthlyBudgetReached`;
4. global daily counter → `DailyLimitReached`;
5. per-client daily counter (keyed IP hash) → `ClientLimitReached`;
6. a concurrency slot within five seconds → `Busy`.

The counters are consumed before the call, so a request that then fails still counts. That
is deliberate: a failed or timed-out call may still have been billed, and a client that
keeps provoking failures must not get unlimited attempts.

## After the call

- The reply is accepted only with stop reason `end_turn` and a clean single line (see
  `AiAnswer`); anything else is `UnexpectedOutput` and the visitor sees the deterministic
  result with an "AI could not answer" note.
- Every call is written to `AiCalls` with its token counts and estimated cost, including
  failures, using `CancellationToken.None`: if the visitor closes the tab mid-call, the
  spend still has to be recorded.
- **Cost** is rounded up to the next micro-dollar, so the budget errs on the side of
  stopping early.

## What it does not do

It never replaces the deterministic result: the API always returns that one, and the AI
answer is shown next to it with an "agrees" or "differs" badge.
