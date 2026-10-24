# DeepLTranslatorTests

The DeepL client against a scripted `HttpMessageHandler`: the request it sends (endpoint
from the key, header, source and target language, XML mode, protected placeholders and
escaped markup), retries on 429 with `Retry-After`, the stop on 456, and giving up after the
last retry. Retry delays are zero so the suite stays fast.
