namespace KodingTask.Api.Messaging;

public interface IWhatsAppSender
{
    Task<string> SendResultAsync(string phoneE164, string locale, string amountText, string words, CancellationToken cancellationToken);
}
