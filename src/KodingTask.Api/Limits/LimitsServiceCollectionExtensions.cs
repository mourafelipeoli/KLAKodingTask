using Amazon;
using Amazon.SimpleSystemsManagement;

namespace KodingTask.Api.Limits;

public static class LimitsServiceCollectionExtensions
{
    public static IServiceCollection AddUsageLimits(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(LimitsOptions.SectionName);
        services.Configure<LimitsOptions>(section);
        services.AddScoped<UsageLimiter>();
        services.AddSingleton<UsageLimitsSource>();

        var settings = section.Get<LimitsOptions>() ?? new LimitsOptions();
        if (!string.IsNullOrWhiteSpace(settings.SsmPath))
        {
            services.AddSingleton<IAmazonSimpleSystemsManagement>(_ => string.IsNullOrWhiteSpace(settings.SsmRegion)
                ? new AmazonSimpleSystemsManagementClient()
                : new AmazonSimpleSystemsManagementClient(RegionEndpoint.GetBySystemName(settings.SsmRegion)));
        }

        return services;
    }
}
