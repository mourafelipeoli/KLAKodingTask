# OutboundMessage

One row per email or WhatsApp message the application tried to send, whatever it was: a
conversion result or a phone code.

- **Masked destination only** (`f*****@example.com`, `+49*******5678`): the history shows
  that a message went out and to roughly whom, without becoming a list of addresses.
- `ProviderMessageId` is the id SES or WhatsApp returned, the key to trace a delivery in
  their consoles; `Error` holds the provider's error code when the send failed.
- Rows older than the retention period are deleted by the retention job.
