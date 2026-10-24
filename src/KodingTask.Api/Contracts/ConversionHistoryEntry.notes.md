# ConversionHistoryEntry

One entry of `GET /api/account/conversions`: the normalised amount, dollars and cents (for
formatting in the interface language), the output language, the words and when.

`ConvertedAt` is a `DateTimeOffset`: the database returns times without a kind, and a plain
`DateTime` would be serialised without an offset and read as local time by the browser.
