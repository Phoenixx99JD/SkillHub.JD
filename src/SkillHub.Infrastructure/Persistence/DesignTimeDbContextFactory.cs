using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SkillHub.Infrastructure.Persistence;

/// <summary>Used by the EF Core CLI (migrations) only.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SkillHubDbContext>
{
    public SkillHubDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SkillHubDbContext>()
            .UseMySql("Server=localhost;Port=3306;Database=skillhub;User=root;Password=root;",
                DependencyInjection.MySqlVersion)
            .Options;
        return new SkillHubDbContext(options);
    }
}
