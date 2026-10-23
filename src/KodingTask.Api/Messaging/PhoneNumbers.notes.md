# PhoneNumbers

Accepts a phone number typed the way people type them (`+49 170 1234-5678`,
`0049 (170) 12345678`) and turns it into E.164 (`+4917012345678`), or refuses it.

- The number must be international: a leading `+` or `00`, then 8 to 15 digits starting with
  a non-zero country code. National formats are refused rather than guessed, because a
  wrong guess would send a code to a stranger.
- Spaces, hyphens, dots and parentheses are ignored.
- Whether the number exists is not checked here: the one-time code proves it.
