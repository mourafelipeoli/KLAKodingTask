# UsageLimits

The caps that protect money and third parties: how many paid operations may run per day,
per client and per month. One snapshot, read through `UsageLimitsSource`.

`Closed` is what the application uses when the limits cannot be read: everything paid is
off. Failing closed means an outage of the parameter store costs some features for a while;
failing open would mean that an outage removes every cap at once.
