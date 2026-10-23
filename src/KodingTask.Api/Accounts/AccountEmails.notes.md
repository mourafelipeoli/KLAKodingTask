# AccountEmails

The two emails an account can trigger for itself: the address confirmation and the password
reset. Both count, in this order, against:

1. the kill switch;
2. `AccountEmailsPerUserPerDay`, so one account cannot loop the confirmation;
3. `EmailsPerDay`, the instance-wide email cap shared with result deliveries.

The per-user cap is checked first so that an account over its own limit does not use up the
shared allowance.
