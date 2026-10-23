using KodingTask.Api.Accounts;
using KodingTask.Api.Contracts;
using KodingTask.Api.Deliveries;
using KodingTask.Api.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KodingTask.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/deliveries")]
[Produces("application/json")]
[EnableRateLimiting(RequestRateLimits.Account)]
[ValidateAntiforgeryHeader]
public sealed class DeliveriesController(
    UserManager<IdentityUser> users,
    DeliveryService deliveries,
    AccountLinks links) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DeliveryResponse>> Send([FromBody] DeliveryRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(User);
        if (user is null)
            return Unauthorized();

        var (response, error) = await deliveries.SendAsync(user, request, links.BaseUrl(HttpContext), cancellationToken);
        return error is null ? response! : StatusCode(ApiErrors.StatusOf(error), error);
    }
}
