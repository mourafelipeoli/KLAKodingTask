namespace KodingTask.Api.Features;

public sealed class PublicSiteOptions
{
    public const string SectionName = "PublicSite";

    public string Url { get; set; } = "https://klakodingtask.mourafelipe.com";

    public string? RepositoryUrl { get; set; }

    public string? AuthorName { get; set; }

    public string? AuthorUrl { get; set; }

    public string? AuthorTerminalUrl { get; set; }
}
