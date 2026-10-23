using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace KodingTask.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseFolder =
        Path.Combine(Path.GetTempPath(), "kodingtask-tests", Guid.NewGuid().ToString("N"));

    public Dictionary<string, string> Settings { get; } = [];

    public List<Action<IServiceCollection>> ServiceOverrides { get; } = [];

    public string DatabaseFile => Path.Combine(_databaseFolder, "kodingtask.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:KodingTask", $"Data Source={DatabaseFile}");
        foreach (var (key, value) in Settings)
            builder.UseSetting(key, value);
        builder.ConfigureTestServices(services =>
        {
            foreach (var overrideServices in ServiceOverrides)
                overrideServices(services);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(_databaseFolder, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
