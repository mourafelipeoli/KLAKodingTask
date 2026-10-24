namespace KodingTask.Api.Translations;

public interface IMachineTranslator
{
    Task<IReadOnlyList<string>> TranslateAsync(IReadOnlyList<string> texts, string locale, CancellationToken cancellationToken);
}

public sealed class MachineTranslationException(string message, bool quotaExceeded = false) : Exception(message)
{
    public bool QuotaExceeded { get; } = quotaExceeded;
}
