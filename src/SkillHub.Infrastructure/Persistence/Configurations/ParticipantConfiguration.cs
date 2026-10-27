using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillHub.Domain.Entities;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Persistence.Configurations;

public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("Participants");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.FullName).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(256).IsRequired();
        builder.Property(p => p.UserId).HasMaxLength(255).IsRequired();

        builder.HasIndex(p => p.Email).IsUnique();
        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasOne<ApplicationUser>().WithOne().HasForeignKey<Participant>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
