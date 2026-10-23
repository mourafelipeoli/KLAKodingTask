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

    public static IServiceCollection AddRequestRateLimits(this IServiceCollection services, IConfiguration configuration)
    {
        var conversionsPerMinute = configuration.GetValue("RateLimits:ConversionsPerMinute", 120);
        var aiPerMinute = configuration.GetValue("RateLimits:AiPerMinute", 10);
        var infrastructurePerMinute = configuration.GetValue("RateLimits:InfrastructurePerMinute", 30);

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

                await context.HttpContext.Response.WriteAsJsonAsync(ErrorResponse.TooManyRequests, cancellationToken);
            };

            options.AddPolicy(Conversion, context => PerClientPerMinute(context, conversionsPerMinute));
            options.AddPolicy(Ai, context => PerClientPerMinute(context, aiPerMinute));
            options.AddPolicy(Infrastructure, context => PerClientPerMinute(context, infrastructurePerMinute));
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
