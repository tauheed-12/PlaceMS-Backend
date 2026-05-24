using CollegeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Constants;

namespace CollegeService.Infrastructure.Persistence.Configurations;

public class CollegeTpoConfiguration : IEntityTypeConfiguration<CollegeTpo>
{
    public void Configure(EntityTypeBuilder<CollegeTpo> builder)
    {
        builder.ToTable("college_tpos");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CollegeId)
            .HasColumnName("college_id")
            .IsRequired();

        builder.Property(c => c.TpoId)
            .HasColumnName("tpo_id")
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("tpo_email")
            .HasMaxLength(SharedKernel.Constants.ValidationRules.EmailMaxLength)
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