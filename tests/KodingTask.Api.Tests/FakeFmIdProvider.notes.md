# FakeFmIdProvider

A stand-in for fmID that keeps the real OpenID Connect handler in the loop: the handler gets
a fixed configuration (instead of the discovery document) and a back-channel that answers
the token request with an id_token signed by a throw-away RSA key.

The browser legs need no server: the authorize and logout requests are redirects, which the
tests read from the `Location` header, and the provider's answer is simulated by calling
`/signin-fmid` with a code and the state from that redirect. The nonce is copied from the
authorize redirect into the next id_token, as the real provider echoes it.

The token request is recorded, so a test can check the client secret and the PKCE verifier
reached the token endpoint.
