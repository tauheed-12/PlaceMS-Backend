using Microsoft.EntityFrameworkCore;
using SharedKernel.Abstractions;
using DriveService.Domain.Entities;

namespace DriveService.Infrastructure.Persistence;

public class DriveDbContext : DbContext
{
    public DriveDbContext(DbContextOptions<DriveDbContext> options) : base(options) { }

    public DbSet<Drive> Drives => Set<Drive>();
    public DbSet<DriveCollege> DriveColleges => Set<DriveCollege>();
    public DbSet<DriveRound> DriveRounds => Set<DriveRound>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("drive");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DriveDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.SetUpdatedAt();
        }
        return base.SaveChangesAsync(ct);
    }
}