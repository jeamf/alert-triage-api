using AlertTriage.Domain;
using Microsoft.EntityFrameworkCore;

namespace AlertTriage.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alert>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Source).HasMaxLength(100).IsRequired();
            e.Property(a => a.Rule).HasMaxLength(200).IsRequired();
            e.Property(a => a.Host).HasMaxLength(255).IsRequired();
            e.Property(a => a.Fingerprint).HasMaxLength(64).IsRequired();
            e.Property(a => a.Severity).HasConversion<string>().HasMaxLength(20);
            e.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(a => a.Fingerprint);
            e.HasIndex(a => a.CreatedAtUtc);
            e.HasIndex(a => a.LastSeenAtUtc);
        });
    }
}
