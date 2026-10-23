using KodingTask.Api.Features;

namespace KodingTask.Api.Contracts;

public sealed record AppInfoResponse(
    string? Version,
    string? Commit,
    string PublicSiteUrl,
    string? RepositoryUrl,
    AuthorResponse? Author,
    FeatureAvailability Features);

public sealed record AuthorResponse(string Name, string Url);
