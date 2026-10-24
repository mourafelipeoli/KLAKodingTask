using System.Globalization;
using System.Threading.RateLimiting;
using KodingTask.Api.Contracts;
using Microsoft.AspNetCore.RateLimiting;

namespace KodingTask.Api.Hosting;

public static class RequestRateLimits
{
    public const string Conversion = "conversion";
    public const string Ai = "ai";
    public const string Infrastructure = "infrastructure";
    public const string Account = "account";
    public const string Translations = "translations";

    public static IServiceCollection AddRequestRateLimits(this IServiceCollection services, IConfiguration configuration)
    {
        var conversionsPerMinute = configuration.GetValue("RateLimits:ConversionsPerMinute", 120);
        var aiPerMinute = configuration.GetValue("RateLimits:AiPerMinute", 10);
        var infrastructurePerMinute = configuration.GetValue("RateLimits:InfrastructurePerMinute", 30);
        var accountPerMinute = configuration.GetValue("RateLimits:AccountPerMinute", 20);
        var translationsPerMinute = configuration.GetValue("RateLimits:TranslationsPerMinute", 30);

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                }

                var texts = context.HttpContext.RequestServices.GetService<Translations.ServerTexts>();
                var error = texts is null ? ErrorResponse.TooManyRequests : TranslateErrorMessages.Translate(ErrorResponse.TooManyRequests, context.HttpContext, texts);
                await context.HttpContext.Response.WriteAsJsonAsync(error, cancellationToken);
            };

            options.AddPolicy(Conversion, context => PerClientPerMinute(context, conversionsPerMinute));
            options.AddPolicy(Ai, context => PerClientPerMinute(context, aiPerMinute));
            options.AddPolicy(Infrastructure, context => PerClientPerMinute(context, infrastructurePerMinute));
            options.AddPolicy(Account, context => PerClientPerMinute(context, accountPerMinute));
            options.AddPolicy(Translations, context => PerClientPerMinute(context, translationsPerMinute));
        });
    }

    internal static string ClientAddress(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static RateLimitPartition<string> PerClientPerMinute(HttpContext context, int permitLimit) =>
        RateLimitPartition.GetFixedWindowLimiter(
            ClientAddress(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            });
}
