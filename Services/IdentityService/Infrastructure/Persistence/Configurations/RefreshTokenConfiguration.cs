using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(rt => rt.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(rt => rt.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token");

        builder.Property(rt => rt.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(rt => rt.IsRevoked)
            .HasColumnName("is_revoked")
            .HasDefaultValue(false);

        builder.Property(rt => rt.RevokedAt)
            .HasColumnName("revoked_at")
            .IsRequired(false);

        builder.Property(rt => rt.RevokedReason)
            .HasColumnName("revoked_reason")
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(rt => rt.RevokedByIp)
            .HasColumnName("revoked_by_ip")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(rt => rt.CreatedByIp)
            .HasColumnName("created_by_ip")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(rt => rt.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(rt => rt.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(rt => new { rt.UserId, rt.IsRevoked })
            .HasDatabaseName("ix_refresh_tokens_user_revoked");
    }
}