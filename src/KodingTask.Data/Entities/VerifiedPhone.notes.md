# VerifiedPhone

The WhatsApp number fmID verified for the account, as of the last sign-in. The number is
managed only on fmID (added there and confirmed with a WhatsApp code); this application
never asks for one. Every sign-in replaces the row with what fmID says, or deletes it when
fmID has no verified number. Results are only ever sent to this number, so nobody can use
the site to message a stranger.

Stored in E.164 (`+4917012345678`), the format the WhatsApp Cloud API expects. It is the
only place a full number is kept: logs and the delivery history hold a masked form.
