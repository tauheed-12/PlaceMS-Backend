using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentService.Domain.Entities;

public class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> builder)
    {
        builder.ToTable("educations");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(e => e.StudentProfileId).HasColumnName("student_profile_id").IsRequired();
        builder.Property(e => e.Degree).HasColumnName("degree").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Institution).HasColumnName("institution").HasMaxLength(200).IsRequired();
        builder.Property(e => e.StartYear).HasColumnName("start_year").IsRequired();
        builder.Property(e => e.EndYear).HasColumnName("end_year").IsRequired(false);
        builder.Property(e => e.Score).HasColumnName("score").HasMaxLength(20).IsRequired(false);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
    }
}