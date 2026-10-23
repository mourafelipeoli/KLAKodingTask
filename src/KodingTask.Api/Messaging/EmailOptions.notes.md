# EmailOptions

Bound from `Email`. Email is on when both `Sender` and `Region` are set; otherwise the
application runs without it and the visitor is handed over to the deployed version.

- `Sender`: the address the emails come from. It must be a verified identity in Amazon SES
  (the address or its domain).
- `SenderName`: the display name next to it.
- `Region`: the SES region where that identity lives.
- `ConfigurationSet`: optional SES configuration set, used to publish delivery metrics to
  CloudWatch, where an alarm can watch the sending volume.

Credentials come from the default AWS chain (the task role when hosted). The role should
only be allowed to send from this address and through this configuration set; see
`deploy/aws/task-role-policy.template.json`.
