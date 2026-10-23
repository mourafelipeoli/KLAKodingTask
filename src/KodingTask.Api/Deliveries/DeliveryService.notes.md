# DeliveryService

Sends a conversion result to the signed-in account itself, by email (to its confirmed
address) or WhatsApp (to its verified number). There is no field for a recipient: the site
can only ever message its own users, about their own conversions.

Order of checks, cheapest and most specific first:

1. the channel is `email` or `whatsapp`, and configured on this instance;
2. the account's email is confirmed (for both channels);
3. the amount and language are valid: **the server converts again** from what the visitor
   typed, so a client can never make the site send arbitrary text;
4. for WhatsApp, the account has a verified number;
5. the kill switch, `DeliveriesPerUserPerDay`, then the channel's instance-wide cap
   (`EmailsPerDay` or `WhatsAppPerDay`).

The words are always the rules engine's, also when the visitor was looking at the AI
answer: what goes out under the site's name is the reference. The email carries a link back
to the same conversion in the visitor's interface language.

The answer gives the masked destination and how many deliveries are left today.
