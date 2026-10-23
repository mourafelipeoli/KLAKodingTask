# MessageLog

Every message the application sends goes through here: it runs the send, then records an
`OutboundMessage` with the masked destination, the provider's message id or error code,
whatever happened. Logs carry the same masked destination, never the address or number.

- The record is saved with `CancellationToken.None`: a visitor who closes the tab after the
  provider accepted the message must not leave a sent message unrecorded.
- A failure answers `SendFailed`; the daily counters stay consumed, which errs on the side
  of sending less.
