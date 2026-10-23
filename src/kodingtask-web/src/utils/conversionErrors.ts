import { textKey } from "../i18n/textKey";
import { NETWORK_ERROR } from "../services/httpClient";

// Server error code → English sentence, which is also the translation key. The sentences
// match the API's own messages, so the UI and the API say the same thing.
const MESSAGES: Readonly<Record<string, string>> = {
  Empty: textKey("Type an amount to convert."),
  TooLong: textKey("The amount is too long."),
  InvalidCharacter: textKey("Only digits, spaces and one comma are allowed."),
  NegativeAmount: textKey("Negative amounts are not supported."),
  DecimalPointNotAllowed: textKey("Use a comma, not a point, to separate the cents."),
  MultipleCommas: textKey("Use only one comma, to separate the cents."),
  MissingDollars: textKey("Type the dollars before the comma, for example 0,50."),
  InvalidCents: textKey("After the comma, type one or two digits for the cents."),
  TooManyCentDigits: textKey("Cents can have at most two digits."),
  InvalidGrouping: textKey(
    "Group thousands in blocks of three digits, for example 45 100.",
  ),
  DollarsOutOfRange: textKey("The maximum is 999 999 999 dollars."),
  UnsupportedLanguage: textKey("This language is not supported."),
  TooManyRequests: textKey(
    "Too many conversions in a short time. Wait a minute and try again.",
  ),
  [NETWORK_ERROR]: textKey(
    "The conversion service is not reachable right now. Try again in a moment.",
  ),
};

export function conversionErrorKey(code: string): string {
  return MESSAGES[code] ?? MESSAGES[NETWORK_ERROR] ?? code;
}
