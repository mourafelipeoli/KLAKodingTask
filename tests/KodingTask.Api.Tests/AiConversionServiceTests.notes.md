# AiConversionServiceTests

The AI engine's guards and bookkeeping, with a fake model and an in-memory database:

- agreement and disagreement with the deterministic words, and the logged cost
  (1 000 input tokens at $0.10/M plus 200 output tokens at $0.50/M is 200 micro-dollars);
- unexpected output (an explanation, digits, a truncated answer, a refusal) is rejected yet
  logged, and so is a failed call;
- no model call at all when the engine is not configured or the kill switch is on;
- the per-client limit trips before the global one, and the global one stops everybody;
- the monthly budget stops the next call once spent;
- the system prompt contains every language and the French regional numbers that differ.
