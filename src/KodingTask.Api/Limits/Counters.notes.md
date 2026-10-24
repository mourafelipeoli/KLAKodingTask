# Counters

The names of the daily counters in `DailyUsage` for messaging and translation, in one place so the code
that consumes a counter and the code that reports it (the account answer, the
infrastructure page) cannot drift apart. The AI engine keeps its own two names next to its
guard.

Per-user counters embed the user id, per-client ones the anonymised client hash; never an
email address or a phone number.

Translation counts characters sent to DeepL (`translation:characters`, one unit per
character) and new terms (`terms:registered`, plus one counter per client).

`whatsapp:sent:month` is the only monthly counter: its row is dated the first day of the
month (see `UsageLimiter.TryConsumeThisMonthAsync`).
