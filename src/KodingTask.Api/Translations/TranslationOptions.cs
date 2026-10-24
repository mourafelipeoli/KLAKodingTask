namespace KodingTask.Api.Translations;

public sealed class TranslationOptions
{
    public const string SectionName = "Translations";

    /// <summary>Imports the bundled dictionaries when the application starts. Tests switch it off and import explicitly.</summary>
    public bool SeedOnStartup { get; set; } = true;
}
