# TranslationBatch

One run of machine translation: for every machine-translated language, the active terms
without a translation in it, 50 at a time (`DeepL:BatchSize`, at most 50, DeepL's limit per
request), until none is missing.

- **Characters are paid**, so each request first consumes its length from the daily
  `TranslationCharactersPerDay` cap, atomically (`UsageLimiter`). When the cap is used up the
  run stops and the rest waits for the next day. The kill switch skips the run.
- Results are stored as `Machine` and **not reviewed**; the review page lists them.
- A `MachineTranslationException` (quota, DeepL unavailable after its retries) ends the run
  quietly; what was saved stays, the rest is retried on the next run.
- A regional variant is never a target (see `InterfaceLanguage`).

This class has no timer; `TranslationBatchService` decides when it runs.
