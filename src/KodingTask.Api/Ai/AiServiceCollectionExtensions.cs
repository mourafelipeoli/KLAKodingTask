using Amazon;
using Amazon.BedrockRuntime;
using KodingTask.Api.Limits;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Ai;

public static class AiServiceCollectionExtensions
{
    public static bool AddAiConversion(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AiOptions.SectionName);
        services.Configure<AiOptions>(section);
        services.AddSingleton<AiPromptBuilder>();
        services.AddSingleton<AiConcurrencyGate>();
        services.AddScoped<AiConversionService>();

        var settings = section.Get<AiOptions>() ?? new AiOptions();
        if (!settings.IsConfigured)
        {
            services.AddSingleton<IAiModelClient, UnavailableAiModelClient>();
            return false;
        }

        services.AddSingleton<IAmazonBedrockRuntime>(_ => new AmazonBedrockRuntimeClient(new AmazonBedrockRuntimeConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region),
            Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds),
            MaxErrorRetry = 2,
        }));
        services.AddSingleton<IAiModelClient, BedrockModelClient>();
        return true;
    }

    private sealed class UnavailableAiModelClient : IAiModelClient
    {
        public Task<AiModelReply> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("The AI engine is not configured.");
    }
}
