using System.Security.Cryptography;
using System.Text;
using KodingTask.Api.Contracts;
using KodingTask.Api.Hosting;
using KodingTask.Api.Limits;
using KodingTask.Api.Messaging;
using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Accounts;

public sealed class PhoneVerification(
    IWhatsAppSender whatsApp,
    IOptions<WhatsAppOptions> options,
    UsageLimitsSource limitsSource,
    UsageLimiter limiter,
    MessageLog log,
    KodingTaskDbContext database,
    ClientFingerprint hasher,
    TimeProvider timeProvider)
{
    public static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
    public const int MaxFailedAttempts = 5;

    public async Task<SendOutcome> SendCodeAsync(string userId, string phoneE164, string locale, CancellationToken cancellationToken)
    {
        if (!options.Value.IsConfigured)
            return SendOutcome.Refused(ApiErrors.MessagingUnavailable);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var challenge = await database.PhoneChallenges.FindAsync([userId], cancellationToken);
        if (challenge is not null && now - challenge.SentAtUtc < ResendCooldown)
            return SendOutcome.Refused(ApiErrors.CodeCooldown);

        var limits = await limitsSource.GetAsync(cancellationToken);
        if (limits.KillSwitch)
            return SendOutcome.Refused(ApiErrors.Paused);
        if (!await limiter.TryConsumeAsync(Counters.PhoneCodes(userId), limits.PhoneCodesPerUserPerDay, cancellationToken))
            return SendOutcome.Refused(ApiErrors.UserLimitReached);
        if (!await limiter.TryConsumeAsync(Counters.WhatsAppMessages, limits.WhatsAppPerDay, cancellationToken))
            return SendOutcome.Refused(ApiErrors.DailyLimitReached);

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
        if (challenge is null)
        {
            challenge = new PhoneChallenge { UserId = userId, PhoneE164 = phoneE164, CodeHash = string.Empty };
            database.PhoneChallenges.Add(challenge);
        }

        challenge.PhoneE164 = phoneE164;
        challenge.CodeHash = HashOf(userId, code);
        challenge.SentAtUtc = now;
        challenge.ExpiresAtUtc = now + CodeLifetime;
        challenge.FailedAttempts = 0;
        await database.SaveChangesAsync(cancellationToken);

        return await log.SendAsync(userId, MessageChannel.WhatsApp, MessageKind.PhoneCode, DestinationMask.Phone(phoneE164),
            token => whatsApp.SendCodeAsync(phoneE164, locale, code, token), cancellationToken);
    }

    public async Task<ErrorResponse?> VerifyAsync(string userId, string? code, CancellationToken cancellationToken)
    {
        var challenge = await database.PhoneChallenges.FindAsync([userId], cancellationToken);
        if (challenge is null)
            return ApiErrors.NoPendingCode;
        if (challenge.FailedAttempts >= MaxFailedAttempts)
            return ApiErrors.TooManyAttempts;
        if (timeProvider.GetUtcNow().UtcDateTime > challenge.ExpiresAtUtc)
            return ApiErrors.CodeExpired;

        var expected = Encoding.ASCII.GetBytes(challenge.CodeHash);
        var actual = Encoding.ASCII.GetBytes(HashOf(userId, (code ?? string.Empty).Trim()));
        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
        {
            challenge.FailedAttempts++;
            await database.SaveChangesAsync(cancellationToken);
            return challenge.FailedAttempts >= MaxFailedAttempts ? ApiErrors.TooManyAttempts : ApiErrors.WrongCode;
        }

        var verified = await database.VerifiedPhones.FindAsync([userId], cancellationToken);
        if (verified is null)
        {
            verified = new VerifiedPhone { UserId = userId, PhoneE164 = challenge.PhoneE164 };
            database.VerifiedPhones.Add(verified);
        }

        verified.PhoneE164 = challenge.PhoneE164;
        verified.VerifiedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        database.PhoneChallenges.Remove(challenge);
        await database.SaveChangesAsync(cancellationToken);
        return null;
    }

    private string HashOf(string userId, string code) => hasher.Of($"phone-code:{userId}:{code}");
}
