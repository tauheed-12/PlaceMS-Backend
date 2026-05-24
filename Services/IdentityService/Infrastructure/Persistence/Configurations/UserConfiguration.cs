using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(u => u.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(ValidationRules.NameMaxLength)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(ValidationRules.EmailMaxLength)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<UserRole>(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.VerificationStatus)
            .HasColumnName("verification_status")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<VerificationStatus>(v))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(u => u.CollegeId)
            .HasColumnName("college_id")
            .IsRequired(false);

        builder.Property(u => u.CollegeCode)
            .HasColumnName("college_code")
            .HasMaxLength(ValidationRules.CollegeCodeMaxLength)
            .IsRequired(false);

        builder.Property(u => u.EmailVerificationToken)
            .HasColumnName("email_verification_token")
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(u => u.EmailVerificationTokenExpiry)
            .HasColumnName("email_verification_token_expiry")
            .IsRequired(false);

        builder.Property(u => u.EmailVerifiedAt)
            .HasColumnName("email_verified_at")
            .IsRequired(false);

        builder.Property(u => u.PasswordResetToken)
            .HasColumnName("password_reset_token")
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(u => u.PasswordResetTokenExpiry)
            .HasColumnName("password_reset_token_expiry")
            .IsRequired(false);

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at")
            .IsRequired(false);

        builder.Property(u => u.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .HasDefaultValue(0);

        builder.Property(u => u.LockoutUntil)
            .HasColumnName("lockout_until")
            .IsRequired(false);

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

        // Refresh tokens — owned collection
        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for common lookup patterns
        builder.HasIndex(u => u.EmailVerificationToken)
            .HasDatabaseName("ix_users_email_verification_token")
            .IsUnique()
            .HasFilter("email_verification_token IS NOT NULL");

        builder.HasIndex(u => u.PasswordResetToken)
            .HasDatabaseName("ix_users_password_reset_token")
            .IsUnique()
            .HasFilter("password_reset_token IS NOT NULL");

        builder.HasIndex(u => u.CollegeCode)
            .HasDatabaseName("ix_users_college_code");

        builder.HasIndex(u => u.Role)
            .HasDatabaseName("ix_users_role");
    }
}