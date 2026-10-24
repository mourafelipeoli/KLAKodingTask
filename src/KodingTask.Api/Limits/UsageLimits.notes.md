# UsageLimits

The caps that protect money and third parties: how many paid operations may run per day,
per client and per month. One snapshot, read through `UsageLimitsSource`.

`Closed` is what the application uses when the limits cannot be read: everything paid is
off. Failing closed means an outage of the parameter store costs some features for a while;
failing open would mean that an outage removes every cap at once.

## Messaging caps

Email and WhatsApp cost money and reach third parties, so every message counts against a
global daily cap for its channel, whatever caused it. On top of that:

| Cap | Default | Counter |
|---|---|---|
| `EmailsPerDay` | 100 | All emails of the instance per UTC day (results). |
| `WhatsAppPerDay` | 50 | All WhatsApp messages per UTC day (results). |
| `DeliveriesPerUserPerDay` | 10 | Results one account may send to itself per UTC day, both channels together. |

A parameter missing from the parameter store reads as zero: the feature it guards is off
until someone who controls the store sets it.

## Translation caps

| Cap | Default | Counter |
|---|---|---|
| `TranslationCharactersPerDay` | 50000 | Characters sent to DeepL per UTC day; a whole new language takes about 7,200. |
| `TermRegistrationsPerDay` | 200 | New texts pages may report per UTC day, all visitors together. |
| `TermRegistrationsPerClientPerDay` | 20 | New texts one client may report per UTC day. |

Reporting texts is free for the visitor but creates rows and, later, paid characters, so
it is capped like the paid features; texts that are terms already cost nothing.
