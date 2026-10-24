using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using KodingTask.Api.Messaging;
using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace KodingTask.Api.Accounts;

public sealed record FmIdSignInResult(IdentityUser? User, string? Error)
{
    public static FmIdSignInResult Refused(string error) => new(null, error);
}

public sealed class FmIdAccounts(
    UserManager<IdentityUser> users,
    KodingTaskDbContext database,
    TimeProvider timeProvider,
    ILogger<FmIdAccounts> logger)
{
    private static readonly EmailAddressAttribute EmailFormat = new();

    public async Task<FmIdSignInResult> ResolveAsync(ClaimsPrincipal external, CancellationToken cancellationToken)
    {
        var result = await FindOrCreateAsync(external);
        if (result.User is not null)
            await SyncPhoneAsync(result.User.Id, external, cancellationToken);
        return result;
    }

    private async Task<FmIdSignInResult> FindOrCreateAsync(ClaimsPrincipal external)
    {
        var subject = external.FindFirstValue("sub") ?? external.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(subject))
            return FmIdSignInResult.Refused(FmIdErrors.InvalidResponse);

        var user = await users.FindByLoginAsync(FmIdOptions.Scheme, subject);
        if (user is not null)
            return new FmIdSignInResult(user, null);

        var email = (external.FindFirstValue("email") ?? external.FindFirstValue(ClaimTypes.Email))?.Trim();
        if (string.IsNullOrEmpty(email) || !EmailFormat.IsValid(email))
            return FmIdSignInResult.Refused(FmIdErrors.InvalidResponse);
        if (!IsTrue(external.FindFirstValue("email_verified")))
            return FmIdSignInResult.Refused(FmIdErrors.NoAccess);

        user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var created = await users.CreateAsync(user);
            if (!created.Succeeded)
                return Failed("create a local account", created);
        }

        var added = await users.AddLoginAsync(user, new UserLoginInfo(FmIdOptions.Scheme, subject, FmIdOptions.Scheme));
        return added.Succeeded ? new FmIdSignInResult(user, null) : Failed("link the fmID subject", added);
    }

    private async Task SyncPhoneAsync(string userId, ClaimsPrincipal external, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var phone = IsTrue(external.FindFirstValue("phone_number_verified"))
            && PhoneNumbers.TryNormalize(external.FindFirstValue("phone_number"), out var phoneE164)
                ? phoneE164
                : null;

        var stored = await database.VerifiedPhones.FindAsync([userId], cancellationToken);
        if (phone is null)
        {
            if (stored is not null)
                database.VerifiedPhones.Remove(stored);
        }
        else if (stored is null)
        {
            database.VerifiedPhones.Add(new VerifiedPhone { UserId = userId, PhoneE164 = phone, VerifiedAtUtc = now });
        }
        else if (stored.PhoneE164 != phone)
        {
            stored.PhoneE164 = phone;
            stored.VerifiedAtUtc = now;
        }

        var profile = await database.UserProfiles.FindAsync([userId], cancellationToken);
        if (profile is null)
        {
            database.UserProfiles.Add(new UserProfile
            {
                UserId = userId,
                PreferredLocale = UserProfiles.DefaultLocale,
                CreatedAtUtc = now,
                PhoneCheckedAtUtc = now,
            });
        }
        else
        {
            profile.PhoneCheckedAtUtc = now;
        }

        await database.SaveChangesAsync(cancellationToken);
    }

    private static bool IsTrue(string? claim) => string.Equals(claim, "true", StringComparison.OrdinalIgnoreCase);

    private FmIdSignInResult Failed(string action, IdentityResult result)
    {
        logger.LogWarning("Could not {Action} for an fmID sign-in: {Errors}", action, string.Join(", ", result.Errors.Select(error => error.Code)));
        return FmIdSignInResult.Refused(FmIdErrors.InvalidResponse);
    }
}
