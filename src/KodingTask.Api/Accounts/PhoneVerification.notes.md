# PhoneVerification

Proves that an account owns a WhatsApp number before any result is sent to it.

**Sending a code** (six random digits from a cryptographic generator, through the WhatsApp
authentication template), in this order of checks:

1. a 60-second cooldown since the last code to this account;
2. the kill switch;
3. `PhoneCodesPerUserPerDay`;
4. `WhatsAppPerDay`, the instance-wide WhatsApp cap shared with results.

Only an HMAC of the code (keyed with `Privacy:ClientHashKey`, bound to the account id) is
stored, with a 10-minute expiry.

**Verifying**: five wrong codes lock the challenge until a new one is requested; the
comparison is constant-time; a correct code stores the number in `VerifiedPhone` and deletes
the challenge. The endpoint is also behind the per-address rate limit, so guessing a
six-digit code within five attempts is hopeless.
