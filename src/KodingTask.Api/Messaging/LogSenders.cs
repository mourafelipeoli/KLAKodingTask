namespace KodingTask.Api.Messaging;

internal sealed class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task<string> SendAsync(string to, ComposedEmail email, CancellationToken cancellationToken)
    {
        logger.LogInformation("[Email:Log] To {To}: {Subject}{NewLine}{Text}", to, email.Subject, Environment.NewLine, email.Text);
        return Task.FromResult($"log-{Guid.NewGuid():N}");
    }
}

internal sealed class LogWhatsAppSender(ILogger<LogWhatsAppSender> logger) : IWhatsAppSender
{
    public Task<string> SendResultAsync(string phoneE164, string locale, string amountText, string words, CancellationToken cancellationToken)
    {
        logger.LogInformation("[WhatsApp:Log] Result to {Phone} ({Locale}): {Amount} = {Words}", phoneE164, locale, amountText, words);
        return Task.FromResult($"log-{Guid.NewGuid():N}");
    }
}
