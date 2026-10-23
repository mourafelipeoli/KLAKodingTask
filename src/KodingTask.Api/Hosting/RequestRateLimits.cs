using System.Threading.RateLimiting;
using KodingTask.Api.Contracts;
using Microsoft.AspNetCore.RateLimiting;

namespace KodingTask.Api.Hosting;

public static class RequestRateLimits
{
    public const string Conversion = "conversion";

    public static IServiceCollection AddRequestRateLimits(this IServiceCollection services, IConfiguration configuration)
    {
        var conversionsPerMinute = configuration.GetValue("RateLimits:ConversionsPerMinute", 120);

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
                await context.HttpContext.Response.WriteAsJsonAsync(ErrorResponse.TooManyRequests, cancellationToken);
            };

            options.AddPolicy(Conversion, context => RateLimitPartition.GetFixedWindowLimiter(
                ClientAddress(context),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = conversionsPerMinute,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));
        });
    }

    internal static string ClientAddress(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
