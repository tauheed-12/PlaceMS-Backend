
using CollegeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace CollegeService.Infrastructure.Persistence.Configurations;

public class CollegeConfiguration : IEntityTypeConfiguration<College>
{
    public void Configure(EntityTypeBuilder<College> builder)
    {
        builder.ToTable("colleges");

        builder.HasKey(c => c.Id);

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .HasMaxLength(ValidationRules.CollegeNameMaxLength)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(ValidationRules.EmailMaxLength)
            .IsRequired();

        // In the database, create a named unique index on the email column so lookups are fast and 
        // duplicate emails are impossible at the database level.
        builder.HasIndex(c => c.Email)
            .IsUnique()
            .HasDatabaseName("ix_colleges_email");

        builder.Property(c => c.Code)
            .HasColumnName("code")
            .HasMaxLength(ValidationRules.CollegeCodeMaxLength)
            .IsRequired();

        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasDatabaseName("ix_colleges_code");

        builder.Property(c => c.City)
            .HasColumnName("city")
            .HasMaxLength(ValidationRules.CollegeCityMaxLength)
            .IsRequired();

        builder.Property(c => c.State)
            .HasColumnName("state")
            .HasMaxLength(ValidationRules.CollegeStateMaxLength)
            .IsRequired();

        builder.Property(c => c.AffiliatedBy)
            .HasColumnName("affiliated_by")
            .HasMaxLength(ValidationRules.CollegeAffiliatedByMaxLength)
            .IsRequired();

        builder.Property(c => c.RegisteredBy)
            .HasColumnName("registered_by");

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<CollegeType>(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.VerificationStatus)
            .HasColumnName("verification_status")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<VerificationStatus>(v))
            .HasMaxLength(30)
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

