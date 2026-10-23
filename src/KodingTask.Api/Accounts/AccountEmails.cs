using KodingTask.Api.Contracts;
using KodingTask.Api.Limits;
using KodingTask.Api.Messaging;
using KodingTask.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Accounts;

public sealed class AccountEmails(
    IEmailSender sender,
    EmailComposer composer,
    IOptions<EmailOptions> options,
    UsageLimitsSource limitsSource,
    UsageLimiter limiter,
    MessageLog log)
{
    public Task<SendOutcome> SendConfirmationAsync(IdentityUser user, string locale, string link, CancellationToken cancellationToken) =>
        SendAsync(user, MessageKind.EmailConfirmation, composer.Confirmation(locale, link), cancellationToken);

    public Task<SendOutcome> SendPasswordResetAsync(IdentityUser user, string locale, string link, CancellationToken cancellationToken) =>
        SendAsync(user, MessageKind.PasswordReset, composer.PasswordReset(locale, link), cancellationToken);

    private async Task<SendOutcome> SendAsync(IdentityUser user, MessageKind kind, ComposedEmail email, CancellationToken cancellationToken)
    {
        if (!options.Value.IsConfigured || string.IsNullOrWhiteSpace(user.Email))
            return SendOutcome.Refused(ApiErrors.MessagingUnavailable);

        var limits = await limitsSource.GetAsync(cancellationToken);
        if (limits.KillSwitch)
            return SendOutcome.Refused(ApiErrors.Paused);
        if (!await limiter.TryConsumeAsync(Counters.AccountEmails(user.Id), limits.AccountEmailsPerUserPerDay, cancellationToken))
            return SendOutcome.Refused(ApiErrors.UserLimitReached);
        if (!await limiter.TryConsumeAsync(Counters.Emails, limits.EmailsPerDay, cancellationToken))
            return SendOutcome.Refused(ApiErrors.DailyLimitReached);

        var to = user.Email;
        return await log.SendAsync(user.Id, MessageChannel.Email, kind, DestinationMask.Email(to),
            token => sender.SendAsync(to, email, token), cancellationToken);
    }
}
