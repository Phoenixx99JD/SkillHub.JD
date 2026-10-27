using SkillHub.Application.Common;
using SkillHub.Application.Common.Exceptions;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Services.Interfaces;
using SkillHub.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace SkillHub.Web.Pages.Workshops;

public class DetailsModel(IWorkshopService workshopService, IEnrollmentService enrollmentService) : AppPageModel
{
    public WorkshopDto Workshop { get; private set; } = null!;
    public bool IsEnrolled { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        try
        {
            Workshop = await workshopService.GetByIdAsync(id, ct);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        if (User.IsInRole(Roles.Participant))
        {
            var mine = await enrollmentService.GetMineAsync(ct);
            IsEnrolled = mine.Any(e => e.WorkshopId == id && e.Status == "Active");
        }

        return Page();
    }
}
