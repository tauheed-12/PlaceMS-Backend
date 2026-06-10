using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(n => n.RecipientUserId).HasColumnName("recipient_user_id").IsRequired();
        builder.Property(n => n.RecipientEmail).HasColumnName("recipient_email").HasMaxLength(256).IsRequired();
        builder.Property(n => n.RecipientName).HasColumnName("recipient_name").HasMaxLength(100).IsRequired();
        builder.Property(n => n.Type).HasColumnName("type")
            .HasConversion(v => v.ToString(), v => Enum.Parse<NotificationType>(v))
            .HasMaxLength(60).IsRequired();
        builder.Property(n => n.Channel).HasColumnName("channel")
            .HasConversion(v => v.ToString(), v => Enum.Parse<NotificationChannel>(v))
            .HasMaxLength(20).IsRequired();
        builder.Property(n => n.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(n => n.Body).HasColumnName("body").HasMaxLength(2000).IsRequired();
        builder.Property(n => n.HtmlBody).HasColumnName("html_body").IsRequired(false);
        builder.Property(n => n.ActionUrl).HasColumnName("action_url").HasMaxLength(500).IsRequired(false);
        builder.Property(n => n.ReferenceId).HasColumnName("reference_id").HasMaxLength(100).IsRequired(false);
        builder.Property(n => n.ReferenceType).HasColumnName("reference_type").HasMaxLength(50).IsRequired(false);
        builder.Property(n => n.CorrelationId).HasColumnName("correlation_id").HasMaxLength(100).IsRequired();
        builder.Property(n => n.Status).HasColumnName("status")
            .HasConversion(v => v.ToString(), v => Enum.Parse<NotificationStatus>(v))
            .HasMaxLength(20).IsRequired();
        builder.Property(n => n.RetryCount).HasColumnName("retry_count").HasDefaultValue(0);
        builder.Property(n => n.SentAt).HasColumnName("sent_at").IsRequired(false);
        builder.Property(n => n.ReadAt).HasColumnName("read_at").IsRequired(false);
        builder.Property(n => n.FailureReason).HasColumnName("failure_reason").HasMaxLength(500).IsRequired(false);
        builder.Property(n => n.CreatedAt).HasColumnName("created_at");
        builder.Property(n => n.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(n => n.RecipientUserId).HasDatabaseName("ix_notifications_recipient_user_id");
        builder.HasIndex(n => new { n.RecipientUserId, n.Status }).HasDatabaseName("ix_notifications_user_status");
        builder.HasIndex(n => new { n.Status, n.RetryCount }).HasDatabaseName("ix_notifications_pending_retry");
    }
}

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(p => p.NotificationType).HasColumnName("notification_type")
            .HasConversion(v => v.ToString(), v => Enum.Parse<NotificationType>(v))
            .HasMaxLength(60).IsRequired();
        builder.Property(p => p.EmailEnabled).HasColumnName("email_enabled").HasDefaultValue(true);
        builder.Property(p => p.InAppEnabled).HasColumnName("in_app_enabled").HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(p => new { p.UserId, p.NotificationType })
            .IsUnique().HasDatabaseName("ix_notification_preferences_user_type");
    }
}