using KodingTask.Api.Contracts;
using KodingTask.Api.Features;
using KodingTask.Api.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Controllers;

[ApiController]
[Route("api/app-info")]
[Produces("application/json")]
public sealed class AppInfoController(
    IConfiguration configuration,
    IOptions<PublicSiteOptions> publicSite,
    FeatureAvailability features) : ControllerBase
{
    [HttpGet]
    public AppInfoResponse Get()
    {
        var site = publicSite.Value;
        var author = string.IsNullOrWhiteSpace(site.AuthorName) || string.IsNullOrWhiteSpace(site.AuthorUrl)
            ? null
            : new AuthorResponse(site.AuthorName, site.AuthorUrl);

        return new(
            configuration.Version(),
            configuration.Commit(),
            site.Url,
            string.IsNullOrWhiteSpace(site.RepositoryUrl) ? null : site.RepositoryUrl,
            author,
            features);
    }
}
