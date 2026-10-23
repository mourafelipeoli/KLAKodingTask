using System.Globalization;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Limits;

public sealed class UsageLimitsSource(
    IOptions<LimitsOptions> options,
    TimeProvider timeProvider,
    ILogger<UsageLimitsSource> logger,
    IAmazonSimpleSystemsManagement? parameterStore = null)
{
    private static readonly TimeSpan FailureRetryDelay = TimeSpan.FromSeconds(10);

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private UsageLimits? _cached;
    private DateTimeOffset _expiresAt;

    public async Task<UsageLimits> GetAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.SsmPath) || parameterStore is null)
            return FromConfiguration(settings);

        if (_cached is not null && timeProvider.GetUtcNow() < _expiresAt)
            return _cached;

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (_cached is not null && timeProvider.GetUtcNow() < _expiresAt)
                return _cached;

            try
            {
                _cached = await ReadParameterStoreAsync(settings.SsmPath, cancellationToken);
                _expiresAt = timeProvider.GetUtcNow().AddSeconds(Math.Max(5, settings.CacheSeconds));
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogError("[Limits] Could not read the limits from {Path} ({Error}); paid features are closed.", settings.SsmPath, exception.GetType().Name);
                _cached = UsageLimits.Closed;
                _expiresAt = timeProvider.GetUtcNow().Add(FailureRetryDelay);
            }

            return _cached;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static UsageLimits FromConfiguration(LimitsOptions settings) => new()
    {
        KillSwitch = settings.KillSwitch,
        AiCallsPerDay = settings.AiCallsPerDay,
        AiCallsPerClientPerDay = settings.AiCallsPerClientPerDay,
        AiMonthlyBudgetUsd = settings.AiMonthlyBudgetUsd,
        EmailsPerDay = settings.EmailsPerDay,
        WhatsAppPerDay = settings.WhatsAppPerDay,
        DeliveriesPerUserPerDay = settings.DeliveriesPerUserPerDay,
        PhoneCodesPerUserPerDay = settings.PhoneCodesPerUserPerDay,
        AccountEmailsPerUserPerDay = settings.AccountEmailsPerUserPerDay,
        RegistrationsPerClientPerDay = settings.RegistrationsPerClientPerDay,
    };

    private async Task<UsageLimits> ReadParameterStoreAsync(string path, CancellationToken cancellationToken)
    {
        var prefix = path.EndsWith('/') ? path : path + "/";
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? nextToken = null;
        do
        {
            var page = await parameterStore!.GetParametersByPathAsync(new GetParametersByPathRequest
            {
                Path = prefix.TrimEnd('/'),
                Recursive = false,
                WithDecryption = true,
                NextToken = nextToken,
            }, cancellationToken);
            foreach (var parameter in page.Parameters ?? [])
                values[parameter.Name[prefix.Length..]] = parameter.Value;
            nextToken = page.NextToken;
        }
        while (!string.IsNullOrEmpty(nextToken));

        return new UsageLimits
        {
            KillSwitch = Read(values, nameof(UsageLimits.KillSwitch), bool.Parse, true),
            AiCallsPerDay = Read(values, nameof(UsageLimits.AiCallsPerDay), ParseInt, 0),
            AiCallsPerClientPerDay = Read(values, nameof(UsageLimits.AiCallsPerClientPerDay), ParseInt, 0),
            AiMonthlyBudgetUsd = Read(values, nameof(UsageLimits.AiMonthlyBudgetUsd), ParseDecimal, 0m),
            EmailsPerDay = Read(values, nameof(UsageLimits.EmailsPerDay), ParseInt, 0),
            WhatsAppPerDay = Read(values, nameof(UsageLimits.WhatsAppPerDay), ParseInt, 0),
            DeliveriesPerUserPerDay = Read(values, nameof(UsageLimits.DeliveriesPerUserPerDay), ParseInt, 0),
            PhoneCodesPerUserPerDay = Read(values, nameof(UsageLimits.PhoneCodesPerUserPerDay), ParseInt, 0),
            AccountEmailsPerUserPerDay = Read(values, nameof(UsageLimits.AccountEmailsPerUserPerDay), ParseInt, 0),
            RegistrationsPerClientPerDay = Read(values, nameof(UsageLimits.RegistrationsPerClientPerDay), ParseInt, 0),
        };
    }

    private static T Read<T>(Dictionary<string, string> values, string name, Func<string, T> parse, T whenMissing) =>
        values.TryGetValue(name, out var raw) ? parse(raw.Trim()) : whenMissing;

    private static int ParseInt(string raw) => int.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture);

    private static decimal ParseDecimal(string raw) => decimal.Parse(raw, NumberStyles.Number, CultureInfo.InvariantCulture);
}
