using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Messaging;

public sealed class SesEmailSender(IAmazonSimpleEmailServiceV2 ses, IOptions<EmailOptions> options) : IEmailSender
{
    public async Task<string> SendAsync(string to, ComposedEmail email, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var response = await ses.SendEmailAsync(new SendEmailRequest
        {
            FromEmailAddress = string.IsNullOrWhiteSpace(settings.SenderName)
                ? settings.Sender
                : $"\"{settings.SenderName}\" <{settings.Sender}>",
            Destination = new Destination { ToAddresses = [to] },
            ConfigurationSetName = string.IsNullOrWhiteSpace(settings.ConfigurationSet) ? null : settings.ConfigurationSet,
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content { Data = email.Subject, Charset = "UTF-8" },
                    Body = new Body
                    {
                        Html = new Content { Data = email.Html, Charset = "UTF-8" },
                        Text = new Content { Data = email.Text, Charset = "UTF-8" },
                    },
                },
            },
        }, cancellationToken);
        return response.MessageId;
    }
}
