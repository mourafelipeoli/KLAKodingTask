# ICurrencySpeller

One implementation per output language: given a valid amount, write the whole sentence
("forty-five thousand one hundred dollars").

The contract is deliberately "amount to sentence", not "number to words". Each language
has its own rules where the number meets the currency: German writes *ein Dollar* but
*einhunderteins Dollar*, Portuguese inserts *de* after an exact million (*um milhão de
dólares*), French keeps the singular for zero (*zéro dollar*), Italian elides vowels.
Splitting "cardinal" and "currency phrase" into two shared abstractions would push those
rules into flags on a common class. Keeping the whole sentence in one class per language
keeps each language readable on its own and testable against native examples.

Implementations must be stateless and thread-safe: the catalog shares one instance of
each across all requests.
