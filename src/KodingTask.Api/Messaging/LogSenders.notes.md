# LogSenders

Development-only senders that write the message to the application log instead of sending
it (`Email:Mode=Log`, `WhatsApp:Mode=Log`). They let anyone try the whole account and
delivery flow on a laptop: register, open the confirmation link from the console, verify a
number with the code from the console, send results.

They refuse to start outside the Development environment
(`MessagingServiceCollectionExtensions`): in production they would write password-reset
links and phone codes into the logs.
