namespace KodingTask.Api.Hosting;

public static class SecurityHeaders
{
    public static IApplicationBuilder UseSecurityHeaders(this WebApplication app, IEnumerable<string>? extraFormActionOrigins = null)
    {
        var contentSecurityPolicy = app.Environment.IsDevelopment()
            ? null
            : BuildContentSecurityPolicy(extraFormActionOrigins ?? []);

        return app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers.XContentTypeOptions = "nosniff";
                headers.XFrameOptions = "DENY";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
                headers["Cross-Origin-Opener-Policy"] = "same-origin";
                if (contentSecurityPolicy is not null)
                    headers.ContentSecurityPolicy = contentSecurityPolicy;
                return Task.CompletedTask;
            });
            await next(context);
        });
    }

    internal static string BuildContentSecurityPolicy(IEnumerable<string> extraFormActionOrigins)
    {
        var formAction = string.Join(' ', new[] { "'self'" }.Concat(extraFormActionOrigins));
        return string.Join("; ",
            "default-src 'self'",
            "script-src 'self'",
            "style-src 'self' 'unsafe-inline'",
            "img-src 'self' data:",
            "font-src 'self'",
            "connect-src 'self'",
            "object-src 'none'",
            "base-uri 'self'",
            "frame-ancestors 'none'",
            $"form-action {formAction}");
    }
}
