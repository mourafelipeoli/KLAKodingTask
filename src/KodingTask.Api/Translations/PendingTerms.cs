using System.Collections.Concurrent;

namespace KodingTask.Api.Translations;

/// <summary>Texts the server used without knowing them yet; the batch stores them as terms.</summary>
public sealed class PendingTerms
{
    private const int Capacity = 1000;
    private readonly ConcurrentDictionary<string, byte> _texts = new(StringComparer.Ordinal);

    public void Add(string text)
    {
        if (text.Length is > 0 and <= 1000 && _texts.Count < Capacity)
            _texts.TryAdd(text, 0);
    }

    public IReadOnlyList<string> Drain()
    {
        var drained = _texts.Keys.ToList();
        foreach (var text in drained)
            _texts.TryRemove(text, out _);
        return drained;
    }
}
