# ReturnUrls

Where a visitor goes back to after signing in or confirming the email: only a path on this
site. Anything else (`//evil.example`, `/\evil.example`, an absolute URL) becomes `/`, so a
crafted link can never use the site to redirect someone elsewhere.
