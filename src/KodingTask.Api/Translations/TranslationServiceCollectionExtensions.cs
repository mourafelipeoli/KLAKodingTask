using Microsoft.Extensions.Options;

namespace KodingTask.Api.Translations;

public static class TranslationServiceCollectionExtensions
{
    /// <summary>The dictionaries are always served; machine translation only when DeepL is configured. Returns whether it is.</summary>
    public static bool AddTranslations(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(DeepLOptions.SectionName);
        services.Configure<DeepLOptions>(section);
        services.Configure<TranslationOptions>(configuration.GetSection(TranslationOptions.SectionName));
        services.AddSingleton(BakedDictionaries.Load());
        services.AddSingleton<TranslationIndex>();
        services.AddSingleton<TranslationBatchSignal>();
        services.AddSingleton<PendingTerms>();
        services.AddSingleton<ServerTexts>();
        services.AddScoped<TranslationSeeder>();
        services.AddScoped<TermRegistry>();
        services.AddScoped<TranslationBatch>();
        services.AddHostedService<TranslationBatchService>();
        services.AddHttpClient<IMachineTranslator, DeepLTranslator>((provider, http) =>
            http.Timeout = TimeSpan.FromSeconds(Math.Max(1, provider.GetRequiredService<IOptions<DeepLOptions>>().Value.TimeoutSeconds)));

        return (section.Get<DeepLOptions>() ?? new DeepLOptions()).IsConfigured;
    }
}
