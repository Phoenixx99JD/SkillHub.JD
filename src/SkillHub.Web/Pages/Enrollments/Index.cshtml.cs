using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Services.Interfaces;
using SkillHub.Web.Infrastructure;

namespace SkillHub.Web.Pages.Enrollments;

public class IndexModel(IEnrollmentService enrollmentService) : AppPageModel
{
    public IReadOnlyList<EnrollmentDto> Enrollments { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Enrollments = await enrollmentService.GetMineAsync(ct);
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id, CancellationToken ct)
    {
        EnrollmentDto? cancelled = null;
        if (await TryExecuteAsync(async () => cancelled = await enrollmentService.CancelAsync(id, ct)))
            SuccessMessage = $"Your enrollment in {cancelled!.WorkshopCode} · {cancelled.WorkshopTitle} was cancelled.";
        else
            KeepErrorsForRedirect();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetCertificateAsync(Guid id, CancellationToken ct)
    {
        CertificateFile? certificate = null;
        if (await TryExecuteAsync(async () => certificate = await enrollmentService.GetCertificateAsync(id, ct)))
            return File(certificate!.Content, certificate.ContentType, certificate.FileName);

        KeepErrorsForRedirect();
        return RedirectToPage();
    }
}
