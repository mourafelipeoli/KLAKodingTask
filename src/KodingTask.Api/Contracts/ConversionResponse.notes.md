# ConversionResponse

A successful conversion as the client sees it.

- `Amount`: the input as received (trimmed), so the client can tell which request this
  answers.
- `NormalizedAmount`, `Dollars`, `Cents`: how the server understood it. The UI shows the
  amount formatted for the user's locale next to the words, which makes a misread input
  (`25,1` is ten cents) visible at a glance.
- `Language`: the canonical code actually used.
- `Engine`: `Deterministic` for the rule-based speller. The AI engine answers with the same
  shape and its own engine name, so the UI can show both side by side.
