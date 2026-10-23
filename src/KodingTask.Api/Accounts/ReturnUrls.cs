namespace KodingTask.Api.Accounts;

public static class ReturnUrls
{
    public static string Sanitize(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl)
        && returnUrl.StartsWith('/')
        && !returnUrl.StartsWith("//", StringComparison.Ordinal)
        && !returnUrl.StartsWith("/\\", StringComparison.Ordinal)
        && !returnUrl.Contains("://", StringComparison.Ordinal)
            ? returnUrl
            : "/";
}
