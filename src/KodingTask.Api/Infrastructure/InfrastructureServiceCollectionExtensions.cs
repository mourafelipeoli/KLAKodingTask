namespace KodingTask.Api.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureReport(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InfrastructureOptions>(configuration.GetSection(InfrastructureOptions.SectionName));
        services.AddHttpClient<ITaskMetadataSource, TaskMetadataClient>(client => client.Timeout = TimeSpan.FromSeconds(2));
        services.AddSingleton<IAwsInfrastructureProbe, AwsInfrastructureProbe>();
        services.AddSingleton<InfrastructureSnapshotSource>();
        services.AddScoped<InfrastructureReporter>();
        return services;
    }
}
