using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;

namespace SkillHub.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.CancelledByUserId).HasMaxLength(255);
        builder.Property(e => e.CancelledByEmail).HasMaxLength(256);

        builder.HasOne(e => e.Participant).WithMany().HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.WorkshopId, e.Status });
        builder.HasIndex(e => new { e.ParticipantId, e.Status });

        builder.Ignore(e => e.IsActive);
    }
}
