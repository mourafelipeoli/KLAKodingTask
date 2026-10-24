using KodingTask.Data.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KodingTask.Data;

public class KodingTaskDbContext(DbContextOptions<KodingTaskDbContext> options)
    : IdentityDbContext<IdentityUser>(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public DbSet<AiCall> AiCalls => Set<AiCall>();

    public DbSet<DailyUsage> DailyUsage => Set<DailyUsage>();

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<VerifiedPhone> VerifiedPhones => Set<VerifiedPhone>();

    public DbSet<OutboundMessage> OutboundMessages => Set<OutboundMessage>();

    public DbSet<ConversionRecord> ConversionRecords => Set<ConversionRecord>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(64);
        configurationBuilder.Properties<decimal>().HavePrecision(18, 6);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("UserProfiles");
            entity.HasKey(profile => profile.UserId);
            entity.Property(profile => profile.UserId).HasMaxLength(255);
            entity.Property(profile => profile.PreferredLocale).HasMaxLength(16);
        });

        modelBuilder.Entity<VerifiedPhone>(entity =>
        {
            entity.ToTable("VerifiedPhones");
            entity.HasKey(phone => phone.UserId);
            entity.Property(phone => phone.UserId).HasMaxLength(255);
            entity.Property(phone => phone.PhoneE164).HasMaxLength(20);
        });

        modelBuilder.Entity<OutboundMessage>(entity =>
        {
            entity.ToTable("OutboundMessages");
            entity.Property(message => message.UserId).HasMaxLength(255);
            entity.Property(message => message.MaskedDestination).HasMaxLength(80);
            entity.Property(message => message.ProviderMessageId).HasMaxLength(200);
            entity.Property(message => message.Error).HasMaxLength(300);
            entity.HasIndex(message => message.CreatedAtUtc);
            entity.HasIndex(message => message.UserId);
        });

        modelBuilder.Entity<ConversionRecord>(entity =>
        {
            entity.ToTable("ConversionRecords");
            entity.Property(record => record.UserId).HasMaxLength(255);
            entity.Property(record => record.Amount).HasMaxLength(32);
            entity.Property(record => record.LanguageCode).HasMaxLength(16);
            entity.Property(record => record.Words).HasMaxLength(1000);
            entity.HasIndex(record => new { record.UserId, record.Amount, record.LanguageCode }).IsUnique();
            entity.HasIndex(record => new { record.UserId, record.ConvertedAtUtc });
            entity.HasIndex(record => record.ConvertedAtUtc);
        });
    }
}
