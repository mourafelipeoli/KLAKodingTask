using System.Collections.Concurrent;
using KodingTask.Api.Translations;

namespace KodingTask.Api.Tests;

/// <summary>Stands in for DeepL: "[ko-KR] Sign in", and it records what it was asked.</summary>
internal sealed class FakeTranslator : IMachineTranslator
{
    public ConcurrentQueue<(string Locale, IReadOnlyList<string> Texts)> Calls { get; } = new();

    public bool QuotaExceeded { get; set; }

    public Task<IReadOnlyList<string>> TranslateAsync(IReadOnlyList<string> texts, string locale, CancellationToken cancellationToken)
    {
        if (QuotaExceeded)
            throw new MachineTranslationException("quota", quotaExceeded: true);
        Calls.Enqueue((locale, texts));
        return Task.FromResult<IReadOnlyList<string>>([.. texts.Select(text => $"[{locale}] {text}")]);
    }
}
