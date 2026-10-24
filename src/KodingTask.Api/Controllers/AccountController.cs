using KodingTask.Api.Accounts;
using KodingTask.Api.Contracts;
using KodingTask.Api.Hosting;
using KodingTask.Api.Limits;
using KodingTask.Api.Messaging;
using KodingTask.Data;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Controllers;

[ApiController]
[Route("api/account")]
[Produces("application/json")]
[EnableRateLimiting(RequestRateLimits.Account)]
[ValidateAntiforgeryHeader]
public sealed class AccountController(
    UserManager<IdentityUser> users,
    IAntiforgery antiforgery,
    UsageLimitsSource limitsSource,
    UsageLimiter limiter,
    IOptions<AdminOptions> admins,
    KodingTaskDbContext database) : ControllerBase
{
    [HttpGet("antiforgery")]
    public AntiforgeryResponse Antiforgery() =>
        new(antiforgery.GetAndStoreTokens(HttpContext).RequestToken!);

    [HttpGet("me")]
    public async Task<AccountResponse> Me(CancellationToken cancellationToken) =>
        await DescribeAsync(await users.GetUserAsync(User), cancellationToken);

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
            Math.Max(0, limits.DeliveriesPerUserPerDay - used),
            admins.Value.IsAdmin(user.Email));
    }
}
