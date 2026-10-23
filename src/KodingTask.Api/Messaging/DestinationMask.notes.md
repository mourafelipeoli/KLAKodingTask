# DestinationMask

How a destination appears anywhere outside the message itself (the delivery history, the
API answer, the screen): enough for the owner to recognise it, not enough to collect it.

- Email: first letter, a run of stars, the domain: `f*****@example.com`. The run length is
  clamped so it does not reveal the length of the local part exactly.
- Phone: country prefix and the last four digits: `+49*******5678`.
