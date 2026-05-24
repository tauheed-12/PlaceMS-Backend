using CollegeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeService.Infrastructure.Persistence.Configurations;

public class AdminCollegeScopeConfig : IEntityTypeConfiguration<AdminCollegeScope>
{
    public void Configure(EntityTypeBuilder<AdminCollegeScope> builder)
    {
        builder.ToTable("admin_colleges");

        builder.Property(c => c.CollegeId)
            .HasColumnName("college_id")
            .IsRequired();

        builder.Property(c => c.AdminUserId)
            .HasColumnName("admin_id")
            .IsRequired();

        builder.Property(u => u.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at")
            .IsRequired(false);

        builder.Property(u => u.DeletedBy)
            .HasColumnName("deleted_by")
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        // Global query filter — soft deleted users are invisible by default
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}