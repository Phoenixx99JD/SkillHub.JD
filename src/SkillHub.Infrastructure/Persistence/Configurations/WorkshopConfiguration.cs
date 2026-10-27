using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillHub.Domain.Entities;

namespace SkillHub.Infrastructure.Persistence.Configurations;

public class WorkshopConfiguration : IEntityTypeConfiguration<Workshop>
{
    public void Configure(EntityTypeBuilder<Workshop> builder)
    {
        builder.ToTable("Workshops", t =>
        {
            t.HasCheckConstraint("CK_Workshops_Capacity", "`Capacity` > 0");
            t.HasCheckConstraint("CK_Workshops_Schedule", "`EndsAt` > `StartsAt`");
        });
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedNever();
        builder.Property(w => w.Code).HasMaxLength(20).IsRequired();
        builder.Property(w => w.Title).HasMaxLength(150).IsRequired();
        builder.Property(w => w.Description).HasMaxLength(2000).IsRequired();
        builder.Property(w => w.Version).IsConcurrencyToken();

        builder.HasIndex(w => w.Code).IsUnique();
        builder.HasIndex(w => w.StartsAt);

        builder.HasOne(w => w.Instructor).WithMany().HasForeignKey(w => w.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Enrollments).WithOne(e => e.Workshop).HasForeignKey(e => e.WorkshopId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(w => w.Enrollments).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(w => w.ActiveEnrollmentsCount);
        builder.Ignore(w => w.AvailableSeats);
    }
}
