using Microsoft.AspNetCore.HttpOverrides;

namespace KodingTask.Api.Hosting;

public static class ForwardedHeadersSetup
{
    public static IServiceCollection AddLoadBalancerForwardedHeaders(this IServiceCollection services) =>
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });
}
