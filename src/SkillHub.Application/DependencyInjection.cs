using Microsoft.Extensions.DependencyInjection;
using SkillHub.Application.Services.Implementations;
using SkillHub.Application.Services.Interfaces;

namespace SkillHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWorkshopService, WorkshopService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IInstructorService, InstructorService>();
        return services;
    }
}
