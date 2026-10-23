# IEmailSender

Sends one composed email to one address and returns the provider's message id. The
production implementation is `SesEmailSender`; tests replace it with a recorder, so no test
ever reaches SES. Callers own the caps and the delivery log; the sender only sends.
