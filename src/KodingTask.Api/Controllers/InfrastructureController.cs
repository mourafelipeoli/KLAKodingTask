using KodingTask.Api.Hosting;
using KodingTask.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KodingTask.Api.Controllers;

[ApiController]
[Route("api/infrastructure")]
[Produces("application/json")]
public sealed class InfrastructureController(InfrastructureReporter reporter) : ControllerBase
{
    [HttpGet]
    [EnableRateLimiting(RequestRateLimits.Infrastructure)]
    [ProducesResponseType<InfrastructureReport>(StatusCodes.Status200OK)]
    public Task<InfrastructureReport> Get(CancellationToken cancellationToken) =>
        reporter.BuildAsync(Request.IsHttps, cancellationToken);
}
