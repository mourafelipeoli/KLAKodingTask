using System.Text.Json;

namespace KodingTask.Api.Hosting;

public static class ApplicationVersion
{
    public const string VersionKey = "App:Version";
    public const string CommitKey = "BUILD_COMMIT";
    public const string BuildDateKey = "BUILD_DATE";

    private const string VersionsFileName = "versions.json";
    private const string VersionProperty = "versionKodingTask";

    public static ConfigurationManager AddApplicationVersion(this ConfigurationManager configuration, string baseDirectory)
    {
        if (!string.IsNullOrWhiteSpace(configuration[VersionKey]))
            return configuration;

        var path = Path.Combine(baseDirectory, VersionsFileName);
        if (!File.Exists(path))
            return configuration;

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.TryGetProperty(VersionProperty, out var element)
                && element.GetString() is { Length: > 0 } version)
            {
                configuration[VersionKey] = version;
            }
        }
        catch (IOException)
        {
        }
        catch (JsonException)
        {
        }

        return configuration;
    }

    public static string? Version(this IConfiguration configuration) => configuration[VersionKey];

    public static string? Commit(this IConfiguration configuration) => configuration[CommitKey];
}
