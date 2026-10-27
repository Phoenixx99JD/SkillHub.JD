using Microsoft.Extensions.Configuration;
using SkillHub.Application.Abstractions.Services;

namespace SkillHub.Infrastructure.Services;

/// <summary>Current time in the time zone configured in "SkillHub:TimeZone" (default America/Bogota).</summary>
public class LocalDateTimeProvider(IConfiguration configuration) : IDateTimeProvider
{
    private readonly TimeZoneInfo _timeZone =
        TimeZoneInfo.FindSystemTimeZoneById(configuration["SkillHub:TimeZone"] ?? "America/Bogota");

    public DateTime Now => DateTime.SpecifyKind(
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone), DateTimeKind.Unspecified);
}
