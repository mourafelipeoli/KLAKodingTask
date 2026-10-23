using System.Security.Cryptography;
using System.Text;
using KodingTask.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KodingTask.Api.Hosting;

public static class DatabaseSetup
{
    private const string ConnectionStringName = "KodingTask";

    public static IServiceCollection AddKodingTaskDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var options = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"The connection string 'ConnectionStrings:{ConnectionStringName}' is not configured.");

        if (options.Provider == DatabaseProvider.Sqlite)
            connectionString = AnchorSqliteFile(connectionString, environment.ContentRootPath);

        services.AddSingleton(options);
        services.AddDbContext<KodingTaskDbContext>(builder =>
            KodingTaskDbContextOptions.Configure(builder, options, connectionString));
        return services;
    }

    public static async Task PrepareDatabaseAsync(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<DatabaseOptions>();
        if (options.Provider != DatabaseProvider.Sqlite)
            return;

        await using var scope = app.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>().Database;
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseSetup));

        var dataSource = new SqliteConnectionStringBuilder(database.GetConnectionString()).DataSource;
        var fingerprintPath = dataSource + ".model";
        var fingerprint = ModelFingerprint(database.GenerateCreateScript());

        if (File.Exists(dataSource) && (!File.Exists(fingerprintPath) || File.ReadAllText(fingerprintPath) != fingerprint))
        {
            logger.LogWarning("[Database] The local SQLite schema is out of date; recreating {DataSource}.", dataSource);
            await database.EnsureDeletedAsync();
        }

        if (await database.EnsureCreatedAsync())
            logger.LogInformation("[Database] Created the local SQLite database {DataSource}.", dataSource);
        await File.WriteAllTextAsync(fingerprintPath, fingerprint);
    }

    private static string AnchorSqliteFile(string connectionString, string contentRoot)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (builder.DataSource is ":memory:" || builder.Mode == SqliteOpenMode.Memory)
            return connectionString;

        if (!Path.IsPathRooted(builder.DataSource))
            builder.DataSource = Path.Combine(contentRoot, builder.DataSource);
        Directory.CreateDirectory(Path.GetDirectoryName(builder.DataSource)!);
        return builder.ToString();
    }

    private static string ModelFingerprint(string createScript) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(createScript)));
}
