# ConversionResult

Outcome of one conversion: either the words or the reason there are none.

Refusals are values, not exceptions. Invalid input is the normal case for a form that
people type into, and the caller (the API) has to turn every refusal into a 400 with a code
anyway. The private constructor and the two factories keep the two states from mixing:
a success always has words and no error, a failure always has an error and no words, and
the `MemberNotNullWhen` attributes let the compiler see that.
