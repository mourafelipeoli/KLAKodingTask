# AdminOptions

Who may review translations: the email addresses in `Admin:Emails`, compared without case.
An address is only trusted because fmID verified it before the account was created here
(see `FmIdAccounts`). The list is configuration, empty by default, so a fresh clone has no
administrator: add your fmID address to `Admin:Emails` (user secrets) to open the review
page locally.

`AccountResponse.IsAdmin` tells the web client whether to show the link; the API checks the
policy on every call regardless.
