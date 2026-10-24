# ConversionHistoryController

`/api/account/conversions`, for the signed-in visitor only (401 otherwise).

| Endpoint | What it does |
|---|---|
| `GET` | The last conversions, newest first (at most ten). |
| `DELETE` | Clears them (204). |

- **The list is written elsewhere**: `POST /api/conversions` records each conversion of a
  signed-in visitor (`ConversionHistory`). There is no endpoint to add an entry, so a client
  cannot store words the server did not produce.
- **Conversion rate limit**, not the account one: the page reads the list after every
  conversion, so it must allow as many reads as conversions.
- Antiforgery on `DELETE` like every other state-changing account call.
