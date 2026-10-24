# LogSenders

Development-only senders that write the message to the application log instead of sending
it (`Email:Mode=Log`, `WhatsApp:Mode=Log`). Together with a development sign-in
client, they let the delivery flow run on a laptop: sign in, verify a number with the code
from the console, send results.

They refuse to start outside the Development environment
(`MessagingServiceCollectionExtensions`): in production they would write phone codes and
other people's results into the logs.
