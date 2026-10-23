using Amazon.Runtime;
using KodingTask.Api.Contracts;
using KodingTask.Data;
using KodingTask.Data.Entities;

namespace KodingTask.Api.Messaging;

public sealed class MessageLog(KodingTaskDbContext database, TimeProvider timeProvider, ILogger<MessageLog> logger)
{
    public async Task<SendOutcome> SendAsync(
        string? userId,
        MessageChannel channel,
        MessageKind kind,
        string maskedDestination,
        Func<CancellationToken, Task<string>> send,
        CancellationToken cancellationToken)
    {
        string? providerMessageId = null;
        string? error = null;
        try
        {
            providerMessageId = await send(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            error = exception switch
            {
                WhatsAppException whatsApp => $"whatsapp:{whatsApp.Code}",
                AmazonServiceException aws => aws.ErrorCode,
                _ => exception.GetType().Name,
            };
            logger.LogWarning("[Messaging] {Channel} {Kind} to {Destination} failed: {Error}.", channel, kind, maskedDestination, error);
        }

        database.OutboundMessages.Add(new OutboundMessage
        {
            CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
            UserId = userId,
            Channel = channel,
            Kind = kind,
            MaskedDestination = maskedDestination,
            Status = error is null ? MessageStatus.Sent : MessageStatus.Failed,
            ProviderMessageId = providerMessageId,
            Error = error,
        });
        await database.SaveChangesAsync(CancellationToken.None);

        if (error is null)
            logger.LogInformation("[Messaging] {Channel} {Kind} sent to {Destination}.", channel, kind, maskedDestination);
        return error is null ? SendOutcome.Sent(maskedDestination) : SendOutcome.Refused(ApiErrors.SendFailed);
    }
}
