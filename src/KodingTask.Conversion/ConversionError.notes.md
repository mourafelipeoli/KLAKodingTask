# ConversionError

Every reason a conversion can be refused, as a closed set.

The API serialises the member name (`"InvalidGrouping"`) and the client translates it into
the user's language. That is why the library carries no human-readable messages: the
wording belongs to the UI dictionaries, and the code stays the contract.

Some members exist only to give a precise hint instead of a generic "invalid input":

- `DecimalPointNotAllowed`: someone typed `25.10`. The task fixes the comma as the cent
  separator, so the message tells them to use a comma instead of guessing.
- `InvalidGrouping`: spaces are accepted as thousands separators (`45 100`), but only in
  blocks of three after the first group. `4 5100` is ambiguous and is refused.
- `MissingDollars`: `,50` is refused; the task's own example writes `0,01`.
- `InvalidCents`: nothing, or something other than digits, after the comma.
