using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using KodingTask.Api.Limits;
using Microsoft.AspNetCore.Identity;

namespace KodingTask.Api.Accounts;

public sealed record FmIdSignInResult(IdentityUser? User, string? Error)
{
    public static FmIdSignInResult Refused(string error) => new(null, error);
}

public sealed class FmIdAccounts(
    UserManager<IdentityUser> users,
    UsageLimitsSource limitsSource,
    UsageLimiter limiter,
    ILogger<FmIdAccounts> logger)
{
    private static readonly EmailAddressAttribute EmailFormat = new();

    public async Task<FmIdSignInResult> ResolveAsync(ClaimsPrincipal external, string clientFingerprint, CancellationToken cancellationToken)
    {
        var subject = external.FindFirstValue("sub") ?? external.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(subject))
            return FmIdSignInResult.Refused(FmIdErrors.InvalidResponse);

        var user = await users.FindByLoginAsync(FmIdOptions.Scheme, subject);
        if (user is null)
        {
            var email = (external.FindFirstValue("email") ?? external.FindFirstValue(ClaimTypes.Email))?.Trim();
            if (string.IsNullOrEmpty(email) || !EmailFormat.IsValid(email))
                return FmIdSignInResult.Refused(FmIdErrors.InvalidResponse);
            if (!string.Equals(external.FindFirstValue("email_verified"), "true", StringComparison.OrdinalIgnoreCase))
                return FmIdSignInResult.Refused(FmIdErrors.NoAccess);

            var linked = await FindOrCreateAsync(email, clientFingerprint, cancellationToken);
            if (linked.Error is not null)
                return linked;
            user = linked.User!;

            var added = await users.AddLoginAsync(user, new UserLoginInfo(FmIdOptions.Scheme, subject, FmIdOptions.Scheme));
            if (!added.Succeeded)
            {
                logger.LogWarning("Could not link an fmID subject to a local account: {Errors}", string.Join(", ", added.Errors.Select(error => error.Code)));
                return FmIdSignInResult.Refused(FmIdErrors.InvalidResponse);
            }
        }

        if (await users.IsLockedOutAsync(user))
            return FmIdSignInResult.Refused(FmIdErrors.LockedOut);

        return new FmIdSignInResult(user, null);
    }

    private async Task<FmIdSignInResult> FindOrCreateAsync(string email, string clientFingerprint, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is not null)
        {
            if (!user.EmailConfirmed)
                await TakeOverUnconfirmedAsync(user);
            return new FmIdSignInResult(user, null);
        }

        var limits = await limitsSource.GetAsync(cancellationToken);
        if (limits.KillSwitch)
            return FmIdSignInResult.Refused(FmIdErrors.Paused);
        if (!await limiter.TryConsumeAsync(Counters.Registrations(clientFingerprint), limits.RegistrationsPerClientPerDay, cancellationToken))
            return FmIdSignInResult.Refused(FmIdErrors.RegistrationLimit);

        user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var created = await users.CreateAsync(user);
        if (!created.Succeeded)
        {
            logger.LogWarning("Could not create a local account for an fmID sign-in: {Errors}", string.Join(", ", created.Errors.Select(error => error.Code)));
            return FmIdSignInResult.Refused(FmIdErrors.InvalidResponse);
        }

        return new FmIdSignInResult(user, null);
    }

    // Whoever registered this address with a password never proved they own it; fmID just
    // did. The password goes and the security stamp changes, so the earlier registrant can
    // neither sign in again nor keep a session.
    private async Task TakeOverUnconfirmedAsync(IdentityUser user)
    {
        user.EmailConfirmed = true;
        if (await users.HasPasswordAsync(user))
            await users.RemovePasswordAsync(user);
        else
            await users.UpdateSecurityStampAsync(user);
    }
}
