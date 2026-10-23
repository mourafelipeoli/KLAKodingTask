# VerifiedPhone

The WhatsApp number an account proved it owns, by typing the one-time code sent to it.
One per account; verifying another number replaces it. Results are only ever sent to this
number, so nobody can use the site to message a stranger.

Stored in E.164 (`+4917012345678`), the format the WhatsApp Cloud API expects. It is the
only place a full number is kept: logs and the delivery history hold a masked form.
