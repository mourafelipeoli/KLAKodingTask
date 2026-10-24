# AccountContracts

Requests and answers of `/api/account` and `/api/deliveries`.

- `AccountResponse` is what the client knows about the visitor: signed in or not, the
  email and whether it is confirmed, the verified WhatsApp number **masked**, and how many
  deliveries are left today. A full phone number never leaves the server again after the
  visitor typed it. `SignedInWithFmId` tells the client that signing out must also end the
  session at fmID.
- Every request carries `UiLocale` where the server writes something to the visitor (an
  email, a WhatsApp message), so the message speaks the language of the page they were on.
- A delivery carries the amount as typed and the output language, never the words: the
  server converts again, so a client cannot make the site send arbitrary text.
- `FmIdForm` is the form body of the fmID sign-in and sign-out posts: the return path and
  the interface language.
