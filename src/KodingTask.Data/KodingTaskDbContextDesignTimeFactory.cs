using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KodingTask.Data;

public sealed class KodingTaskDbContextDesignTimeFactory : IDesignTimeDbContextFactory<KodingTaskDbContext>
{
    public KodingTaskDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<KodingTaskDbContext>();
        KodingTaskDbContextOptions.Configure(
            builder,
            new DatabaseOptions { Provider = DatabaseProvider.MySql },
            "Server=design-time-only;Database=kodingtask");
        return new KodingTaskDbContext(builder.Options);
    }
}
