using KodingTask.Api.Features;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Accounts;

public sealed class AccountLinks(IOptions<PublicSiteOptions> publicSite, IHostEnvironment environment)
{
    public string BaseUrl(HttpContext context) =>
        environment.IsDevelopment()
            ? $"{context.Request.Scheme}://{context.Request.Host}"
            : publicSite.Value.Url.TrimEnd('/');

    public static string SignInPath(string error, string? returnUrl) =>
        $"/sign-in?error={Uri.EscapeDataString(error)}&returnUrl={Uri.EscapeDataString(ReturnUrls.Sanitize(returnUrl))}";

    public static string SignInAndStartPath(string? returnUrl) =>
        $"/sign-in?returnUrl={Uri.EscapeDataString(ReturnUrls.Sanitize(returnUrl))}&with=fmid";

    public static string ConversionPath(string amount, string language, string uiLocale) =>
        $"/?amount={Uri.EscapeDataString(amount)}&language={Uri.EscapeDataString(language)}&lang={Uri.EscapeDataString(uiLocale)}";
}
