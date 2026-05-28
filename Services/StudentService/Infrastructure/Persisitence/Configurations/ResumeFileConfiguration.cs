using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentService.Domain.Entities;

public class ResumeFileConfiguration : IEntityTypeConfiguration<ResumeFile>
{
    public void Configure(EntityTypeBuilder<ResumeFile> builder)
    {
        builder.ToTable("resume_files");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(r => r.StudentProfileId).HasColumnName("student_profile_id").IsRequired();
        builder.Property(r => r.OriginalFileName).HasColumnName("original_file_name").HasMaxLength(255).IsRequired();
        builder.Property(r => r.StoredObjectName).HasColumnName("stored_object_name").HasMaxLength(500).IsRequired();
        builder.Property(r => r.BucketName).HasColumnName("bucket_name").HasMaxLength(100).IsRequired();
        builder.Property(r => r.FileSizeBytes).HasColumnName("file_size_bytes").IsRequired();
        builder.Property(r => r.ContentType).HasColumnName("content_type").HasMaxLength(100).IsRequired();
        builder.Property(r => r.IsActive).HasColumnName("is_active").HasDefaultValue(false);
        builder.Property(r => r.UploadedAt).HasColumnName("uploaded_at");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(r => new { r.StudentProfileId, r.IsActive })
            .HasDatabaseName("ix_resume_files_profile_active");
    }
}