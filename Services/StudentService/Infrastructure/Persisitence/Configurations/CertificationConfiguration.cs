using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentService.Domain.Entities;

public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> builder)
    {
        builder.ToTable("certifications");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.StudentProfileId).HasColumnName("student_profile_id").IsRequired();
        builder.Property(c => c.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(c => c.IssuingOrganization).HasColumnName("issuing_organization").HasMaxLength(200).IsRequired();
        builder.Property(c => c.IssueDate).HasColumnName("issue_date").IsRequired();
        builder.Property(c => c.ExpiryDate).HasColumnName("expiry_date").IsRequired(false);
        builder.Property(c => c.CredentialUrl).HasColumnName("credential_url").HasMaxLength(500).IsRequired(false);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");
    }
}