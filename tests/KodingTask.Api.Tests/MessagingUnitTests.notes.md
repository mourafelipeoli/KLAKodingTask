# MessagingUnitTests

Pure checks of the messaging helpers: the email texts complete in every language with the
same placeholders (read from the bundled dictionaries), the regional French fallback,
encoding in the email, address and phone masking, phone normalisation, Meta's language
codes and safe return URLs. The two that need `ServerTexts` take it from a test host,
because it depends on the translation cache.
