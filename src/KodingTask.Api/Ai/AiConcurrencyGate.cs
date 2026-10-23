using Microsoft.Extensions.Options;

namespace KodingTask.Api.Ai;

public sealed class AiConcurrencyGate(IOptions<AiOptions> options) : IDisposable
{
    private static readonly TimeSpan MaxWait = TimeSpan.FromSeconds(5);

    private readonly SemaphoreSlim _slots = new(Math.Max(1, options.Value.MaxConcurrency));

    public Task<bool> TryEnterAsync(CancellationToken cancellationToken) => _slots.WaitAsync(MaxWait, cancellationToken);

    public void Release() => _slots.Release();

    public void Dispose() => _slots.Dispose();
}
