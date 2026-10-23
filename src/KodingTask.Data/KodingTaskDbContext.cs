using KodingTask.Data.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KodingTask.Data;

public class KodingTaskDbContext(DbContextOptions<KodingTaskDbContext> options)
    : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public DbSet<AiCall> AiCalls => Set<AiCall>();

    public DbSet<DailyUsage> DailyUsage => Set<DailyUsage>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(64);
        configurationBuilder.Properties<decimal>().HavePrecision(18, 6);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiCall>(entity =>
        {
            entity.ToTable("AiCalls");
            entity.Property(call => call.Model).HasMaxLength(200);
            entity.Property(call => call.Language).HasMaxLength(16);
            entity.Property(call => call.ClientHash).HasMaxLength(64);
            entity.Property(call => call.Error).HasMaxLength(300);
            entity.HasIndex(call => call.CreatedAtUtc);
        });

        modelBuilder.Entity<DailyUsage>(entity =>
        {
            entity.ToTable("DailyUsage");
            entity.HasKey(usage => new { usage.Day, usage.Counter });
            entity.Property(usage => usage.Counter).HasMaxLength(96);
        });
    }
}
