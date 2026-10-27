using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillHub.Application.Abstractions.Persistence;
using SkillHub.Application.Abstractions.Services;
using SkillHub.Infrastructure.Identity;
using SkillHub.Infrastructure.Persistence;
using SkillHub.Infrastructure.Persistence.Repositories;
using SkillHub.Infrastructure.Seeding;
using SkillHub.Infrastructure.Services;

namespace SkillHub.Infrastructure;

public static class DependencyInjection
{
    public static readonly MySqlServerVersion MySqlVersion = new(new Version(8, 0, 36));

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<SkillHubDbContext>(options => options.UseMySql(connectionString, MySqlVersion));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<SkillHubDbContext>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IWorkshopRepository, WorkshopRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IInstructorRepository, InstructorRepository>();
        services.AddScoped<IParticipantRepository, ParticipantRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IDateTimeProvider, LocalDateTimeProvider>();
        services.AddSingleton<ICertificateGenerator, QuestPdfCertificateGenerator>();
        services.AddSingleton<IEnrollmentImportReader, ExcelEnrollmentImportReader>();
        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}
