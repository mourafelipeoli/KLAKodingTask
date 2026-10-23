# UnavailableSenders

Stand-ins registered when email or WhatsApp is not configured, so everything that depends on
a sender can still be constructed on a fresh clone. The services check the options before
sending and answer `MessagingUnavailable`, so these are never reached in practice; if one
is, the failure is loud instead of silent.
