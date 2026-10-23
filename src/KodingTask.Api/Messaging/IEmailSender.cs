namespace KodingTask.Api.Messaging;

public interface IEmailSender
{
    Task<string> SendAsync(string to, ComposedEmail email, CancellationToken cancellationToken);
}
