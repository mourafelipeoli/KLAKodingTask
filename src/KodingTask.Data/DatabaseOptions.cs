namespace KodingTask.Data;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;

    public string MySqlServerVersion { get; set; } = "8.4.0";

    public int MySqlMaxRetries { get; set; } = 3;
}
