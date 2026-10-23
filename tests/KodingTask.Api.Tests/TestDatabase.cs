using KodingTask.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KodingTask.Api.Tests;

public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public TestDatabase()
    {
        _connection.Open();
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public KodingTaskDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<KodingTaskDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();
}
