using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace KodingTask.Api.Accounts;

public sealed record FmIdSignInResult(IdentityUser? User, string? Error)
{
    public static FmIdSignInResult Refused(string error) => new(null, error);
}

public sealed class FmIdAccounts(UserManager<IdentityUser> users, ILogger<FmIdAccounts> logger)
{
    private static readonly EmailAddressAttribute EmailFormat = new();

    public async Task<FmIdSignInResult> ResolveAsync(ClaimsPrincipal external)
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
        if (!string.Equals(external.FindFirstValue("email_verified"), "true", StringComparison.OrdinalIgnoreCase))
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

    private FmIdSignInResult Failed(string action, IdentityResult result)
    {
        logger.LogWarning("Could not {Action} for an fmID sign-in: {Errors}", action, string.Join(", ", result.Errors.Select(error => error.Code)));
        return FmIdSignInResult.Refused(FmIdErrors.InvalidResponse);
    }
}
