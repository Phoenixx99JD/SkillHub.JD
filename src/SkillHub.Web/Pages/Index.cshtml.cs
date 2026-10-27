using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Common;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Services.Interfaces;
using SkillHub.Web.Infrastructure;

namespace SkillHub.Web.Pages;

public class IndexModel(IWorkshopService workshopService, IEnrollmentService enrollmentService) : AppPageModel
{
    public IReadOnlyList<WorkshopDto> Workshops { get; private set; } = [];

    /// <summary>Workshops where the current participant already holds an active enrollment.</summary>
    public HashSet<Guid> EnrolledWorkshopIds { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var workshops = await workshopService.GetUpcomingAsync(ct);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            workshops = workshops.Where(w =>
                    w.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    w.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    w.InstructorName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    w.InstructorSpecialty.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        Workshops = workshops;

        if (User.IsInRole(Roles.Participant))
        {
            var mine = await enrollmentService.GetMineAsync(ct);
            EnrolledWorkshopIds = mine.Where(e => e.Status == "Active").Select(e => e.WorkshopId).ToHashSet();
        }
    }

    public async Task<IActionResult> OnPostEnrollAsync(Guid workshopId, CancellationToken ct)
    {
        if (!User.IsInRole(Roles.Participant))
            return User.Identity?.IsAuthenticated == true
                ? RedirectToPage("/Account/AccessDenied")
                : RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Index") });

        EnrollmentDto? enrollment = null;
        if (await TryExecuteAsync(async () => enrollment = await enrollmentService.EnrollCurrentParticipantAsync(
                new CreateEnrollmentRequest { WorkshopId = workshopId }, ct)))
        {
            SuccessMessage = L["Quedaste inscrito en {0} · {1}.", enrollment!.WorkshopCode, enrollment.WorkshopTitle];
        }
        else
        {
            KeepErrorsForRedirect();
        }

        return RedirectToPage(new { Search });
    }
}
