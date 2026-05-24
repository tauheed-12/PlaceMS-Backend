using CollegeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CollegeService.Infrastructure.Persistence;

public class CollegeDbContext : DbContext
{
    public CollegeDbContext(DbContextOptions<CollegeDbContext> options) : base(options) { }

    public DbSet<College> Colleges => Set<College>();
    public DbSet<CollegeTpo> CollegeTpos => Set<CollegeTpo>();
    public DbSet<AdminCollegeScope> AdminCollegeScopes => Set<AdminCollegeScope>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("college");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CollegeDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<SharedKernel.Abstractions.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.SetUpdatedAt();
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}