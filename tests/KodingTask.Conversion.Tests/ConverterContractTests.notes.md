# ConverterContractTests

Properties that hold for every language, independent of grammar:

- the catalog lists exactly the eight supported codes, in display order;
- language codes are normalised (`EN-us`, `fr_be`) and unknown codes are refused before the
  amount is even looked at;
- across the first 2 100 integers plus 3 000 random amounts (fixed seed, so a failure is
  reproducible) and the maximum, every output is non-empty, contains no digit, no double
  space, no double hyphen, no surrounding whitespace, and is the same on a second call.

The sweep catches the class of bug golden tables miss: an empty group producing a stray
space, an index off by one at a decade boundary, a missing word for some rarely-typed
number.
