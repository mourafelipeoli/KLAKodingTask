# LimitsOptions

Bound from the `Limits` section.

- Without `SsmPath`, the values in this section are the limits. That is the mode for a local
  run and for anyone deploying the code without a parameter store.
- With `SsmPath` (for example `/klakodingtask/limits/`), the limits are read from AWS Systems
  Manager Parameter Store, one parameter per property name (`…/AiCallsPerDay`,
  `…/KillSwitch`, …), and the values in this section are ignored. The task role can read
  that path and nothing else; it cannot write it. Whoever controls the parameters controls
  the caps, independently of whoever controls the code.
- `CacheSeconds` bounds how long a changed limit takes to apply (one minute by default) and
  how often the store is read.
