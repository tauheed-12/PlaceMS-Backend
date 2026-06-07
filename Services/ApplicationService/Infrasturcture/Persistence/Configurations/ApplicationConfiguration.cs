using ApplicationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationService.Infrastructure.Persistence.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<StudentApplication>
{
    public void Configure(EntityTypeBuilder<StudentApplication> builder)
    {
        builder.ToTable("applications");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(a => a.DriveId).HasColumnName("drive_id").IsRequired();
        builder.Property(a => a.CollegeId).HasColumnName("college_id").IsRequired();
        builder.Property(a => a.StudentId).HasColumnName("student_id").IsRequired();
        builder.Property(a => a.StudentName).HasColumnName("student_name").HasMaxLength(255).IsRequired();
        builder.Property(a => a.StudentEmail).HasColumnName("student_email").HasMaxLength(255).IsRequired();
        builder.Property(a => a.CollegeName).HasColumnName("college_name").HasMaxLength(255).IsRequired();
        builder.Property(a => a.CompanyName).HasColumnName("company_name").HasMaxLength(255).IsRequired();
        builder.Property(a => a.JobRole).HasColumnName("job_role").HasMaxLength(255).IsRequired();
        builder.Property(a => a.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(a => a.AppliedOn).HasColumnName("applied_on").IsRequired();
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(a => new { a.DriveId, a.StudentId }).IsUnique().HasDatabaseName("ux_applications_drive_student");
        builder.HasIndex(a => new { a.DriveId, a.Status }).HasDatabaseName("ix_applications_drive_status");
        builder.HasIndex(a => a.StudentId).HasDatabaseName("ix_applications_student");
        builder.HasIndex(a => a.CollegeId).HasDatabaseName("ix_applications_college");
        builder.HasIndex(a => a.AppliedOn).HasDatabaseName("ix_applications_applied_on");
    }
}