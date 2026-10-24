using KodingTask.Api.Features;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Translations;

public sealed class TranslationBatchService(
    IServiceScopeFactory scopes,
    TranslationIndex index,
    PendingTerms pending,
    TranslationBatchSignal signal,
    IOptions<DeepLOptions> deepL,
    IOptions<TranslationOptions> translations,
    FeatureAvailability features,
    ILogger<TranslationBatchService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (translations.Value.SeedOnStartup)
            await SeedAsync(stoppingToken);
        var interval = TimeSpan.FromSeconds(Math.Max(10, deepL.Value.IntervalSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<TermRegistry>().AddServerTermsAsync(pending.Drain(), stoppingToken);
                if (features.MachineTranslation)
                    await scope.ServiceProvider.GetRequiredService<TranslationBatch>().RunAsync(stoppingToken);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning("[Translations] The batch failed: {Error}.", exception.GetType().Name);
            }

            try
            {
                await signal.WaitAsync(interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async Task SeedAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<TranslationSeeder>().SeedAsync(stoppingToken);
            index.Invalidate();
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogWarning("[Translations] The bundled dictionaries could not be imported: {Error}.", exception.GetType().Name);
        }
    }
}
