using KodingTask.Api.Contracts;
using KodingTask.Api.History;
using KodingTask.Api.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KodingTask.Api.Controllers;

[ApiController]
[Route("api/account/conversions")]
[Produces("application/json")]
[Authorize]
[EnableRateLimiting(RequestRateLimits.Conversion)]
[ValidateAntiforgeryHeader]
public sealed class ConversionHistoryController(UserManager<IdentityUser> users, ConversionHistory history) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<ConversionHistoryEntry>> List(CancellationToken cancellationToken) =>
        await history.ListAsync(users.GetUserId(User)!, cancellationToken);

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        await history.ClearAsync(users.GetUserId(User)!, cancellationToken);
        return NoContent();
    }
}
