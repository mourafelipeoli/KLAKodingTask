using KodingTask.Api.Accounts;
using KodingTask.Api.Contracts;
using KodingTask.Api.Limits;
using KodingTask.Api.Messaging;
using KodingTask.Conversion;
using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Deliveries;

public sealed class DeliveryService(
    CurrencyConverter converter,
    SpellerCatalog catalog,
    EmailComposer composer,
    IEmailSender emailSender,
    IWhatsAppSender whatsAppSender,
    IOptions<EmailOptions> emailOptions,
    IOptions<WhatsAppOptions> whatsAppOptions,
    UsageLimitsSource limitsSource,
    UsageLimiter limiter,
    UserProfiles profiles,
    MessageLog log,
    KodingTaskDbContext database,
    TimeProvider timeProvider)
{
    public const string EmailChannel = "email";
    public const string WhatsAppChannel = "whatsapp";

    /// <summary>How long a sign-in's word on the WhatsApp number holds before a send asks fmID again.</summary>
    public static readonly TimeSpan PhoneCheckValidity = TimeSpan.FromMinutes(10);

    public async Task<(DeliveryResponse? Response, ErrorResponse? Error)> SendAsync(
        IdentityUser user,
        DeliveryRequest request,
        string baseUrl,
        CancellationToken cancellationToken)
    {
        var channel = request.Channel?.Trim().ToLowerInvariant();
        if (channel is not (EmailChannel or WhatsAppChannel))
            return (null, ApiErrors.InvalidChannel);
        if (channel == EmailChannel ? !emailOptions.Value.IsConfigured : !whatsAppOptions.Value.IsConfigured)
            return (null, ApiErrors.MessagingUnavailable);
        if (!user.EmailConfirmed || string.IsNullOrWhiteSpace(user.Email))
            return (null, ApiErrors.EmailNotConfirmed);

        var conversion = converter.Convert(request.Amount, request.Language);
        if (!conversion.IsSuccess)
            return (null, ErrorResponse.For(conversion.Error.Value));

        VerifiedPhone? phone = null;
        if (channel == WhatsAppChannel)
        {
            var profile = await database.UserProfiles.FindAsync([user.Id], cancellationToken);
            if (profile?.PhoneCheckedAtUtc is not { } checkedAt || timeProvider.GetUtcNow().UtcDateTime - checkedAt > PhoneCheckValidity)
                return (null, ApiErrors.PhoneCheckNeeded);
            phone = await database.VerifiedPhones.FindAsync([user.Id], cancellationToken);
            if (phone is null)
                return (null, ApiErrors.PhoneNotVerified);
        }

        var limits = await limitsSource.GetAsync(cancellationToken);
        if (limits.KillSwitch)
            return (null, ApiErrors.Paused);
        if (!await limiter.TryConsumeAsync(Counters.Deliveries(user.Id), limits.DeliveriesPerUserPerDay, cancellationToken))
            return (null, ApiErrors.UserLimitReached);
        var channelCounter = channel == EmailChannel ? Counters.Emails : Counters.WhatsAppMessages;
        var channelLimit = channel == EmailChannel ? limits.EmailsPerDay : limits.WhatsAppPerDay;
        if (!await limiter.TryConsumeAsync(channelCounter, channelLimit, cancellationToken))
            return (null, ApiErrors.DailyLimitReached);

        var locale = await profiles.RememberLocaleAsync(user.Id, request.UiLocale, cancellationToken);
        var amountText = AmountText.Format(conversion.Amount, locale);
        var words = conversion.Words;
        var outcome = channel == EmailChannel
            ? await SendEmailAsync(user, locale, conversion, amountText, baseUrl, cancellationToken)
            : await log.SendAsync(user.Id, MessageChannel.WhatsApp, MessageKind.Result, DestinationMask.Phone(phone!.PhoneE164),
                token => whatsAppSender.SendResultAsync(phone.PhoneE164, locale, amountText, words, token), cancellationToken);
        if (!outcome.IsSent)
            return (null, outcome.Error);

        var used = await limiter.CountAsync(Counters.Deliveries(user.Id), cancellationToken);
        return (new DeliveryResponse(outcome.MaskedDestination!, Math.Max(0, limits.DeliveriesPerUserPerDay - used)), null);
    }

    private Task<SendOutcome> SendEmailAsync(
        IdentityUser user,
        string locale,
        ConversionResult conversion,
        string amountText,
        string baseUrl,
        CancellationToken cancellationToken)
    {
        catalog.TryGet(conversion.LanguageCode, out var speller);
        var link = baseUrl + AccountLinks.ConversionPath(conversion.Amount.ToCanonicalString(), conversion.LanguageCode, locale);
        var email = composer.Result(locale, amountText, conversion.Words!, conversion.LanguageCode, speller!.Language.NativeName, link);
        var to = user.Email!;
        return log.SendAsync(user.Id, MessageChannel.Email, MessageKind.Result, DestinationMask.Email(to),
            token => emailSender.SendAsync(to, email, token), cancellationToken);
    }
}
