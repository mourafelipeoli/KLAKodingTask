namespace KodingTask.Api.Contracts;

public sealed record InterfaceLanguageResponse(string Code, string NativeName, string? VariantOf);

public sealed record TranslationDictionaryResponse(string Locale, string Version, IReadOnlyDictionary<string, string> Entries);

public sealed record TermRegistrationRequest(IReadOnlyList<string?>? Texts);

public sealed record TermRegistrationResponse(int Added);

public sealed record AdminTranslationRow(
    int TermId,
    string English,
    string Origin,
    string? Text,
    string? Inherited,
    string? Source,
    bool Reviewed,
    DateTimeOffset? UpdatedAt);

public sealed record AdminTranslationUpdate(int TermId, string? Locale, string? Text, bool Reviewed);
