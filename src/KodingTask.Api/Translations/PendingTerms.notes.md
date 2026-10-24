# PendingTerms

The in-memory queue between `ServerTexts`, which must never wait for the database, and the
batch loop, which stores the queued texts as terms. Bounded (1000 texts of up to 1000
characters) so a bug that formats unique texts cannot grow it without limit; what does not
fit is dropped and queued again the next time the text is used.
