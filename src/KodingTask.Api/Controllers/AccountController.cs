using KodingTask.Api.Accounts;
using KodingTask.Api.Contracts;
using KodingTask.Api.Hosting;
using KodingTask.Api.Limits;
using KodingTask.Api.Messaging;
using KodingTask.Data;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KodingTask.Api.Controllers;

[ApiController]
[Route("api/account")]
[Produces("application/json")]
[EnableRateLimiting(RequestRateLimits.Account)]
[ValidateAntiforgeryHeader]
public sealed class AccountController(
    UserManager<IdentityUser> users,
    IAntiforgery antiforgery,
    PhoneVerification phones,
    UserProfiles profiles,
    UsageLimitsSource limitsSource,
    UsageLimiter limiter,
    KodingTaskDbContext database) : ControllerBase
{
    [HttpGet("antiforgery")]
    public AntiforgeryResponse Antiforgery() =>
        new(antiforgery.GetAndStoreTokens(HttpContext).RequestToken!);

    [HttpGet("me")]
    public async Task<AccountResponse> Me(CancellationToken cancellationToken) =>
        await DescribeAsync(await users.GetUserAsync(User), cancellationToken);

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
            phone is null ? null : DestinationMask.Phone(phone.PhoneE164),
            Math.Max(0, limits.DeliveriesPerUserPerDay - used));
    }

    private ObjectResult Refuse(ErrorResponse error) => StatusCode(ApiErrors.StatusOf(error), error);
}
