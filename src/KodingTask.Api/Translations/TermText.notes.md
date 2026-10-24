# TermText

A term is identified by the SHA-256 of its full English text, stored in an indexed,
unique `TextHash` column. Indexing the text itself would limit its length (MySQL indexes
at most 3072 bytes, and a key prefix would make two long texts collide); a normalised
prefix would merge texts that differ only after it.
