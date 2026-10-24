# ConversionHistoryTests

The last conversions through the real endpoints with a visitor signed in by the fake fmID
provider: newest first and once per amount and language (the normalised amount makes "1"
and "1,00" the same), the limit of ten, nothing kept for anonymous visitors, failed
conversions or the `GET` query endpoint, each visitor's list private and clearable, the
antiforgery token on clearing, and the 90-day purge.
