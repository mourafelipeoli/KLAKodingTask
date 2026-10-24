# EmailComposer

Builds the one email the application sends: a conversion result, with a subject, an HTML
part and a plain-text part with the same content (mail clients that block HTML, and spam
filters, both prefer that). Confirming addresses and resetting passwords are fmID's.

- **Written in the visitor's interface language** through `ServerTexts`, with the same
  terms, review and machine translation as the web client; the converted
  words keep their own language (`lang` attribute), which may differ: a German interface
  can send the French words.
- **Table layout with inline styles**, the only thing every mail client renders the same,
  in the colours of the site; no images, so nothing is blocked by default and nothing is
  loaded from the site when the email is opened.
- **Everything dynamic is HTML-encoded**, including the words and the links, so an amount
  or a translation can never inject markup.
- **No amount in the subject.** The subject is a plain "Your Koding Task result"; the amount
  and the words are the heading and the body. A dollar figure in the subject, next to a
  sum spelled out in words and a button link, is what payment scams look like, and Gmail
  filed the first real email as spam for it even though SPF and DKIM passed.
