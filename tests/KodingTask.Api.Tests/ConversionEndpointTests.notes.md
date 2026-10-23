# ConversionEndpointTests

The HTTP contract of the conversion endpoints, through the real pipeline: JSON body and
query string, the English default, the error shape (`400` with `{ error, message }`) for
refusals, the all-languages endpoint (which also proves the Belgian and Swiss variants
reach the client), the language list, and the JSON 404 for unknown API paths instead of
the SPA's `index.html`.
