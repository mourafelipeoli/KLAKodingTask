# MessagingServiceCollectionExtensions

Registers email and WhatsApp and reports which of them are configured, which becomes
`FeatureAvailability`. A channel without configuration gets a stand-in sender, so the rest
of the application never needs to know.

The SES client is only created when email is configured, so a fresh clone never touches the
AWS credential chain.

`Mode=Log` (development only) swaps either channel for a sender that writes to the log;
see `LogSenders`. Starting with it in any other environment fails on purpose.

