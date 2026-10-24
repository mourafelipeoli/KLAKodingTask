# MetaWhatsAppSender

Sends template messages through the WhatsApp Cloud API
(`POST {ApiBaseUrl}/{PhoneNumberId}/messages`, bearer token).

- **Result**: the result template with two body parameters, the amount and the words. It is
  the only template: numbers are verified by fmID, which sends its own codes.
- **Language**: the interface locale mapped by `MetaLanguages`. When Meta answers that the
  template does not exist in that language (error 132001), the message is sent once more in
  the fallback language instead of failing.
- **Errors** become a `WhatsAppException` carrying Meta's error code and the HTTP status;
  the response body is never logged, since it can echo the destination number.
- The `HttpClient` has the configured timeout and no automatic retry: a duplicated message
  is worse than a failed one the visitor can retry.
- The number is sent without the leading `+`, the form Meta's examples use.
