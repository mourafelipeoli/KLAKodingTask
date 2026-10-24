# ConversionRecord

One of a signed-in visitor's last conversions (`ConversionRecords`).

- **Keyed by the user id** like every other per-user table, never by an Identity column
  beyond it.
- **One row per amount and output language** (unique index on user, amount, language): the
  same conversion made again moves to the top instead of filling the list with copies.
- **`Amount` is the normalised amount** ("45 100,00"), so "45100" and "45 100" are the same
  entry, and the converter accepts it as it is when the visitor opens the entry again.
  `Dollars` and `Cents` let the client format the amount in the interface language.
- **`Words` is kept** rather than converted again on every listing: the list is cheap to
  show, and it shows what the visitor actually saw.
- Indexes on (user, time) for the list and on time for the retention purge.
