# PhoneNumbers

Checks a phone number and turns it into E.164 (`+4917012345678`), or refuses it. The number
comes from fmID's `phone_number` claim, already in E.164; passing it through here anyway
means a malformed claim is dropped instead of being sent to Meta.

- The number must be international: a leading `+` or `00`, then 8 to 15 digits starting with
  a non-zero country code. National formats are refused rather than guessed.
- Spaces, hyphens, dots and parentheses are ignored.
