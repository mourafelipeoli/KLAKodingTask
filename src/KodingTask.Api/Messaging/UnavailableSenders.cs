namespace KodingTask.Api.Messaging;

internal sealed class UnavailableEmailSender : IEmailSender
{
    public Task<string> SendAsync(string to, ComposedEmail email, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Email is not configured on this instance.");
}

internal sealed class UnavailableWhatsAppSender : IWhatsAppSender
{
    public Task<string> SendResultAsync(string phoneE164, string locale, string amountText, string words, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("WhatsApp is not configured on this instance.");

    public Task<string> SendCodeAsync(string phoneE164, string locale, string code, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("WhatsApp is not configured on this instance.");
}
