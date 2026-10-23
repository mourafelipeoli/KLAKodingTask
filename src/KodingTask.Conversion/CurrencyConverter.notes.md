# CurrencyConverter

The library's entry point: text and a language code in, `ConversionResult` out.

The language is checked before the amount, so an unknown language is reported as such even
when the amount is also wrong; a client with a bad language code would otherwise chase
amount errors that are not its real problem. The returned `LanguageCode` is the catalog's
canonical spelling (`fr-BE`), whatever casing the caller sent.

The class holds no state besides the catalog and is registered as a singleton by the API.
