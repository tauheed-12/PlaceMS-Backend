using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentService.Domain.Entities;

namespace StudentService.Infrastructure.Persistence.Configurations;

public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.ToTable("student_profiles");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.HasIndex(s => s.UserId).IsUnique().HasDatabaseName("ix_student_profiles_user_id");
        builder.Property(s => s.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
        builder.Property(s => s.Email).HasColumnName("email").HasMaxLength(256).IsRequired();
        builder.HasIndex(s => s.Email).HasDatabaseName("ix_student_profiles_email");
        builder.Property(s => s.PhoneNumber).HasColumnName("phone_number").HasMaxLength(15).IsRequired();
        builder.Property(s => s.CollegeId).HasColumnName("college_id").IsRequired();
        builder.HasIndex(s => s.CollegeId).HasDatabaseName("ix_student_profiles_college_id");
        builder.Property(s => s.CollegeCode).HasColumnName("college_code").HasMaxLength(10).IsRequired();
        builder.Property(s => s.CollegeName).HasColumnName("college_name").HasMaxLength(200).IsRequired();
        builder.Property(s => s.Branch).HasColumnName("branch").HasMaxLength(100).IsRequired(false);
        builder.Property(s => s.BatchYear).HasColumnName("batch_year").HasDefaultValue(0);
        builder.Property(s => s.Cgpa).HasColumnName("cgpa").HasPrecision(4, 2).HasDefaultValue(0.0);
        builder.Property(s => s.AboutMe).HasColumnName("about_me").HasMaxLength(1000).IsRequired(false);
        builder.Property(s => s.ProfileCompletionScore).HasColumnName("profile_completion_score").HasDefaultValue(0);
        builder.Property(s => s.ActiveResumeFileId).HasColumnName("active_resume_file_id").IsRequired(false);
        builder.Property(s => s.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(s => s.DeletedAt).HasColumnName("deleted_at").IsRequired(false);
        builder.Property(s => s.DeletedBy).HasColumnName("deleted_by").HasMaxLength(256).IsRequired(false);
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasMany(s => s.Educations).WithOne()
            .HasForeignKey(e => e.StudentProfileId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Skills).WithOne()
            .HasForeignKey(sk => sk.StudentProfileId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Projects).WithOne()
            .HasForeignKey(p => p.StudentProfileId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Certifications).WithOne()
            .HasForeignKey(c => c.StudentProfileId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.ResumeFiles).WithOne()
            .HasForeignKey(r => r.StudentProfileId).OnDelete(DeleteBehavior.Cascade);
    }
}
