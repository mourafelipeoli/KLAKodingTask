using Microsoft.EntityFrameworkCore;

namespace KodingTask.Data;

public static class KodingTaskDbContextOptions
{
    public static DbContextOptionsBuilder Configure(
        DbContextOptionsBuilder builder,
        DatabaseOptions options,
        string connectionString) => options.Provider switch
        {
            DatabaseProvider.Sqlite => builder.UseSqlite(connectionString),
            DatabaseProvider.MySql => builder.UseMySql(
                connectionString,
                new MySqlServerVersion(Version.Parse(options.MySqlServerVersion)),
                mySql => mySql.EnableRetryOnFailure(options.MySqlMaxRetries)),
            _ => throw new InvalidOperationException($"Unsupported database provider '{options.Provider}'."),
        };
}
