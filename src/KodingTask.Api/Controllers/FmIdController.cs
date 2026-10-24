using System.Security.Claims;
using KodingTask.Api.Accounts;
using KodingTask.Api.Contracts;
using KodingTask.Api.Features;
using KodingTask.Api.Hosting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KodingTask.Api.Controllers;

[ApiController]
[Route("api/account/fmid")]
[ApiExplorerSettings(IgnoreApi = true)]
[EnableRateLimiting(RequestRateLimits.Account)]
public sealed class FmIdController(
    SignInManager<IdentityUser> signIn,
    IAntiforgery antiforgery,
    FmIdAccounts accounts,
    UserProfiles profiles,
    ClientFingerprint fingerprint,
    FeatureAvailability features) : ControllerBase
{
    private const string CompletePath = "/api/account/fmid/complete";

    [HttpPost("sign-in")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> SignInWithFmId([FromForm] FmIdForm form)
    {
        var returnUrl = ReturnUrls.Sanitize(form.ReturnUrl);
        if (!features.ExternalLogin)
            return LocalRedirect(AccountLinks.SignInPath(FmIdErrors.Unavailable, returnUrl));
        if (!await antiforgery.IsRequestValidAsync(HttpContext))
            return LocalRedirect(AccountLinks.SignInPath(FmIdErrors.InvalidResponse, returnUrl));

        var properties = new AuthenticationProperties { RedirectUri = CompletePath };
        properties.Items[FmIdServiceCollectionExtensions.ReturnUrlItem] = returnUrl;
        if (!string.IsNullOrWhiteSpace(form.UiLocale))
            properties.Items[FmIdServiceCollectionExtensions.UiLocaleItem] = form.UiLocale;
        return Challenge(properties, FmIdOptions.Scheme);
    }

    [HttpGet("complete")]
    public async Task<IActionResult> Complete(CancellationToken cancellationToken)
    {
        var external = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        var returnUrl = ReturnUrls.Sanitize(Item(external.Properties, FmIdServiceCollectionExtensions.ReturnUrlItem));
        if (!external.Succeeded)
            return LocalRedirect(AccountLinks.SignInPath(FmIdErrors.InvalidResponse, returnUrl));

        var result = await accounts.ResolveAsync(external.Principal, fingerprint.Of(HttpContext), cancellationToken);
        if (result.User is null)
            return LocalRedirect(AccountLinks.SignInPath(result.Error!, returnUrl));

        await profiles.RememberLocaleAsync(result.User.Id, Item(external.Properties, FmIdServiceCollectionExtensions.UiLocaleItem), cancellationToken);
        await signIn.SignInAsync(result.User, isPersistent: true, authenticationMethod: FmIdOptions.Scheme);
        return LocalRedirect(returnUrl);
    }

    [HttpPost("sign-out")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> SignOutOfFmId([FromForm] FmIdForm form)
    {
        var returnUrl = ReturnUrls.Sanitize(form.ReturnUrl);
        if (!await antiforgery.IsRequestValidAsync(HttpContext))
            return LocalRedirect(returnUrl);

        var signedInWithFmId = User.FindFirstValue(ClaimTypes.AuthenticationMethod) == FmIdOptions.Scheme;
        await signIn.SignOutAsync();
        if (!signedInWithFmId || !features.ExternalLogin)
            return LocalRedirect(returnUrl);
        return SignOut(new AuthenticationProperties { RedirectUri = returnUrl }, FmIdOptions.Scheme);
    }

    private static string? Item(AuthenticationProperties? properties, string key) =>
        properties is not null && properties.Items.TryGetValue(key, out var value) ? value : null;
}
