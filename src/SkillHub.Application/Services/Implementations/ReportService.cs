using SkillHub.Application.Abstractions.Persistence;
using SkillHub.Application.DTOs.Reports;
using SkillHub.Application.Services.Interfaces;

namespace SkillHub.Application.Services.Implementations;

public class ReportService(IWorkshopRepository workshops) : IReportService
{
    public const int MaxTop = 100;

    public async Task<IReadOnlyList<TopWorkshopDto>> GetTopWorkshopsAsync(int top, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, MaxTop);
        var result = await workshops.GetTopByActiveEnrollmentsAsync(top, ct);

        return result
            .Select(x => new TopWorkshopDto(x.Workshop.Id, x.Workshop.Code, x.Workshop.Title,
                x.Workshop.Instructor.FullName, x.Workshop.StartsAt, x.Workshop.Capacity, x.ActiveEnrollments))
            .ToList();
    }
}
