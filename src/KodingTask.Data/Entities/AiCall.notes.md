# AiCall

One row per call to the AI model, successful or not: the audit trail behind the AI budget.

- **Cost in micro-dollars (`long`)**, not `decimal`. Integer sums are exact and SQLite (the
  local provider) cannot aggregate `decimal` on the server; a monthly total is one `SUM`
  query on both providers. One micro-dollar is far below the cost of a single token batch.
- **Tokens are stored by class** (input, output, cache read, cache write) because each has
  its own price; output includes the model's thinking tokens, which are billed as output.
- **`ClientHash`** is a keyed hash of the caller's IP address, never the address itself:
  enough to apply a per-client daily limit, useless to identify a person.
- **`Outcome`** records whether the model agreed with the deterministic speller, disagreed,
  answered something that is not a plausible amount in words, or failed. Failed calls are
  logged too: a refused or timed-out request may still have been billed, and the budget
  must count it.
- No amount text, no words, no prompt: the row is about cost and behaviour, not content.
