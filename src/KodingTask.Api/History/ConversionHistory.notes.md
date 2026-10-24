# ConversionHistory

The last conversions of a signed-in visitor, kept by the server for the account.

- **Recorded by the conversion endpoint itself** (`POST /api/conversions`), only when a
  session is present: the words always come from the server's own conversion, never from
  the client, and anonymous visitors leave nothing behind.
- **Distinct and short**: a conversion already in the list (same normalised amount and
  output language) is moved to the top; anything beyond the newest `Size` (10) is deleted
  on the next write. With the 90-day purge (`RetentionJob`), a person never has more than
  ten rows.
- **Best effort**: recording never fails a conversion. Any database error is logged and the
  visitor gets the words anyway; two simultaneous identical conversions that race on the
  unique index lose one write, which is the same row.
- **Clearing** deletes the visitor's rows at once (`ExecuteDeleteAsync`).
