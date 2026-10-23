# SesEmailSender

Sends through Amazon SES v2 (`SendEmail`, simple content with HTML and text parts, UTF-8).

- The `From` header carries the display name (`"Koding Task" <sender>`). The IAM condition
  `ses:FromAddress` compares only the address, so the task role can pin the sender while
  the name stays configurable.
- The configuration set is passed when configured, which is what attributes the send to
  this application in CloudWatch.
- Errors are not caught here: the caller logs the failed attempt and answers the visitor.
