using System.ComponentModel.DataAnnotations;
using System.Text;
using KodingTask.Api.Accounts;
using KodingTask.Api.Contracts;
using KodingTask.Api.Features;
using KodingTask.Api.Hosting;
using KodingTask.Api.Limits;
using KodingTask.Api.Messaging;
using KodingTask.Data;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;

namespace KodingTask.Api.Controllers;

[ApiController]
[Route("api/account")]
[Produces("application/json")]
[EnableRateLimiting(RequestRateLimits.Account)]
[ValidateAntiforgeryHeader]
public sealed class AccountController(
    UserManager<IdentityUser> users,
    SignInManager<IdentityUser> signIn,
    IAntiforgery antiforgery,
    AccountEmails accountEmails,
    AccountLinks links,
    PhoneVerification phones,
    UserProfiles profiles,
    UsageLimitsSource limitsSource,
    UsageLimiter limiter,
    ClientFingerprint fingerprint,
    FeatureAvailability features,
    KodingTaskDbContext database) : ControllerBase
{
    private static readonly EmailAddressAttribute EmailFormat = new();

    [HttpGet("antiforgery")]
    public AntiforgeryResponse Antiforgery() =>
        new(antiforgery.GetAndStoreTokens(HttpContext).RequestToken!);

    [HttpGet("me")]
    public async Task<AccountResponse> Me(CancellationToken cancellationToken) =>
        await DescribeAsync(await users.GetUserAsync(User), cancellationToken);

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!features.Accounts)
            return Refuse(ApiErrors.MessagingUnavailable);
        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email) || !EmailFormat.IsValid(email))
            return Refuse(ApiErrors.InvalidEmail);
        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 10)
            return Refuse(ApiErrors.WeakPassword);
        if (await users.FindByEmailAsync(email) is not null)
            return Refuse(ApiErrors.EmailTaken);

        var limits = await limitsSource.GetAsync(cancellationToken);
        if (limits.KillSwitch)
            return Refuse(ApiErrors.Paused);
        if (!await limiter.TryConsumeAsync(Counters.Registrations(fingerprint.Of(HttpContext)), limits.RegistrationsPerClientPerDay, cancellationToken))
            return Refuse(ApiErrors.RegistrationLimitReached);

        var user = new IdentityUser { UserName = email, Email = email };
        var created = await users.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            return Refuse(created.Errors.Any(error => error.Code.StartsWith("Duplicate", StringComparison.Ordinal))
                ? ApiErrors.EmailTaken
                : ApiErrors.WeakPassword);

        var locale = await profiles.RememberLocaleAsync(user.Id, request.UiLocale, cancellationToken);
        var sent = await SendConfirmationAsync(user, locale, request.ReturnUrl, cancellationToken);
        await signIn.SignInAsync(user, isPersistent: true);
        return new RegisterResponse(await DescribeAsync(user, cancellationToken), sent.IsSent);
    }

    [HttpPost("confirm-email")]
    public async Task<ActionResult<AccountResponse>> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var user = string.IsNullOrWhiteSpace(request.UserId) ? null : await users.FindByIdAsync(request.UserId);
        var token = Decode(request.Code);
        if (user is null || token is null)
            return Refuse(ApiErrors.InvalidLink);
        if (!user.EmailConfirmed)
        {
            var confirmed = await users.ConfirmEmailAsync(user, token);
            if (!confirmed.Succeeded)
                return Refuse(ApiErrors.InvalidLink);
        }

        await signIn.SignInAsync(user, isPersistent: true);
        return await DescribeAsync(user, cancellationToken);
    }

    [Authorize]
    [HttpPost("resend-confirmation")]
    public async Task<ActionResult<AccountResponse>> ResendConfirmation([FromBody] ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(User);
        if (user is null)
            return Unauthorized();
        if (!user.EmailConfirmed)
        {
            var locale = await profiles.RememberLocaleAsync(user.Id, request.UiLocale, cancellationToken);
            var sent = await SendConfirmationAsync(user, locale, request.ReturnUrl, cancellationToken);
            if (!sent.IsSent)
                return Refuse(sent.Error!);
        }

        return await DescribeAsync(user, cancellationToken);
    }

    [HttpPost("sign-in")]
    public async Task<ActionResult<AccountResponse>> SignIn([FromBody] SignInRequest request, CancellationToken cancellationToken)
    {
        var user = string.IsNullOrWhiteSpace(request.Email) ? null : await users.FindByEmailAsync(request.Email.Trim());
        if (user is null || string.IsNullOrEmpty(request.Password))
            return Refuse(ApiErrors.InvalidCredentials);

        var result = await signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
            return Refuse(ApiErrors.LockedOut);
        if (!result.Succeeded)
            return Refuse(ApiErrors.InvalidCredentials);

        await signIn.SignInAsync(user, isPersistent: true);
        return await DescribeAsync(user, cancellationToken);
    }

    [HttpPost("sign-out")]
    public async Task<AccountResponse> SignOutOfAccount()
    {
        await signIn.SignOutAsync();
        return AccountResponse.SignedOut;
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        if (!features.Accounts)
            return Refuse(ApiErrors.MessagingUnavailable);

        var user = string.IsNullOrWhiteSpace(request.Email) ? null : await users.FindByEmailAsync(request.Email.Trim());
        if (user is not null)
        {
            var locale = await profiles.RememberLocaleAsync(user.Id, request.UiLocale, cancellationToken);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await users.GeneratePasswordResetTokenAsync(user)));
            var link = links.BaseUrl(HttpContext) + AccountLinks.ResetPasswordPath(user.Id, code, locale);
            await accountEmails.SendPasswordResetAsync(user, locale, link, cancellationToken);
        }

        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<AccountResponse>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = string.IsNullOrWhiteSpace(request.UserId) ? null : await users.FindByIdAsync(request.UserId);
        var token = Decode(request.Code);
        if (user is null || token is null)
            return Refuse(ApiErrors.InvalidLink);
        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 10)
            return Refuse(ApiErrors.WeakPassword);

        var reset = await users.ResetPasswordAsync(user, token, request.Password);
        if (!reset.Succeeded)
            return Refuse(reset.Errors.Any(error => error.Code == "InvalidToken") ? ApiErrors.InvalidLink : ApiErrors.WeakPassword);

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await users.UpdateAsync(user);
        }

        await users.ResetAccessFailedCountAsync(user);
        await signIn.SignInAsync(user, isPersistent: true);
        return await DescribeAsync(user, cancellationToken);
    }

    [Authorize]
    [HttpPost("phone/code")]
    public async Task<ActionResult<PhoneCodeResponse>> SendPhoneCode([FromBody] PhoneCodeRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(User);
        if (user is null)
            return Unauthorized();
        if (!user.EmailConfirmed)
            return Refuse(ApiErrors.EmailNotConfirmed);
        if (!PhoneNumbers.TryNormalize(request.Phone, out var phone))
            return Refuse(ApiErrors.InvalidPhone);

        var locale = await profiles.RememberLocaleAsync(user.Id, request.UiLocale, cancellationToken);
        var sent = await phones.SendCodeAsync(user.Id, phone, locale, cancellationToken);
        return sent.IsSent ? new PhoneCodeResponse(sent.MaskedDestination!) : Refuse(sent.Error!);
    }

    [Authorize]
    [HttpPost("phone/verify")]
    public async Task<ActionResult<AccountResponse>> VerifyPhone([FromBody] PhoneVerifyRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(User);
        if (user is null)
            return Unauthorized();

        var error = await phones.VerifyAsync(user.Id, request.Code, cancellationToken);
        return error is null ? await DescribeAsync(user, cancellationToken) : Refuse(error);
    }

    private async Task<SendOutcome> SendConfirmationAsync(IdentityUser user, string locale, string? returnUrl, CancellationToken cancellationToken)
    {
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await users.GenerateEmailConfirmationTokenAsync(user)));
        var link = links.BaseUrl(HttpContext) + AccountLinks.ConfirmEmailPath(user.Id, code, ReturnUrls.Sanitize(returnUrl), locale);
        return await accountEmails.SendConfirmationAsync(user, locale, link, cancellationToken);
    }

    private async Task<AccountResponse> DescribeAsync(IdentityUser? user, CancellationToken cancellationToken)
    {
        if (user is null)
            return AccountResponse.SignedOut;

        var phone = await database.VerifiedPhones.FindAsync([user.Id], cancellationToken);
        var limits = await limitsSource.GetAsync(cancellationToken);
        var used = await limiter.CountAsync(Counters.Deliveries(user.Id), cancellationToken);
        return new AccountResponse(
            true,
            user.Email,
            user.EmailConfirmed,
            phone is null ? null : DestinationMask.Phone(phone.PhoneE164),
            Math.Max(0, limits.DeliveriesPerUserPerDay - used));
    }

    private static string? Decode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;
        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private ObjectResult Refuse(ErrorResponse error) => StatusCode(ApiErrors.StatusOf(error), error);
}
