# SendOutcome

The answer of anything that tries to send a message: either the masked destination it went
to, or the refusal (from `ApiErrors`) that the endpoint passes on as is. Refusals are values,
not exceptions, like everywhere else in the API.
