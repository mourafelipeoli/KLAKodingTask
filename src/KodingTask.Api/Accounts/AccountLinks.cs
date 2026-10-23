using KodingTask.Api.Features;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Accounts;

public sealed class AccountLinks(IOptions<PublicSiteOptions> publicSite, IHostEnvironment environment)
{
    public string BaseUrl(HttpContext context) =>
        environment.IsDevelopment()
            ? $"{context.Request.Scheme}://{context.Request.Host}"
            : publicSite.Value.Url.TrimEnd('/');

    public static string ConfirmEmailPath(string userId, string code, string returnUrl, string uiLocale) =>
        $"/confirm-email?user={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}&returnUrl={Uri.EscapeDataString(ReturnUrls.Sanitize(returnUrl))}&lang={Uri.EscapeDataString(uiLocale)}";

    public static string ResetPasswordPath(string userId, string code, string uiLocale) =>
        $"/reset-password?user={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}&lang={Uri.EscapeDataString(uiLocale)}";

    public static string ConversionPath(string amount, string language, string uiLocale) =>
        $"/?amount={Uri.EscapeDataString(amount)}&language={Uri.EscapeDataString(language)}&lang={Uri.EscapeDataString(uiLocale)}";
}
