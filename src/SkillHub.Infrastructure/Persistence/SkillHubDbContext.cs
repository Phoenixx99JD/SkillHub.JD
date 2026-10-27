using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Persistence;

public class SkillHubDbContext(DbContextOptions<SkillHubDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(SkillHubDbContext).Assembly);
    }
}
