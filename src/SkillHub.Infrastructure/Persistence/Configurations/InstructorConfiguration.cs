using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillHub.Domain.Entities;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Persistence.Configurations;

public class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> builder)
    {
        builder.ToTable("Instructors");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();
        builder.Property(i => i.FullName).HasMaxLength(150).IsRequired();
        builder.Property(i => i.Email).HasMaxLength(256).IsRequired();
        builder.Property(i => i.Specialty).HasMaxLength(100).IsRequired();
        builder.Property(i => i.UserId).HasMaxLength(255).IsRequired();

        builder.HasIndex(i => i.Email).IsUnique();
        builder.HasIndex(i => i.UserId).IsUnique();
        builder.HasOne<ApplicationUser>().WithOne().HasForeignKey<Instructor>(i => i.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
