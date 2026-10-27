using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Common;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Services.Interfaces;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/enrollments")]
[Authorize]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    private const long MaxImportFileSize = 5 * 1024 * 1024;

    /// <summary>Enrolls the authenticated participant in a workshop.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Participant)]
    [ProducesResponseType<EnrollmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EnrollmentDto>> Create(CreateEnrollmentRequest request, CancellationToken ct)
    {
        var enrollment = await enrollmentService.EnrollCurrentParticipantAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, enrollment);
    }

    /// <summary>Lists the enrollments of the authenticated participant.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = Roles.Participant)]
    public async Task<ActionResult<IReadOnlyList<EnrollmentDto>>> GetMine(CancellationToken ct) =>
        Ok(await enrollmentService.GetMineAsync(ct));

    /// <summary>Cancels an enrollment (its owner or an employee).</summary>
    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = $"{Roles.Participant},{Roles.Employee}")]
    [ProducesResponseType<EnrollmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EnrollmentDto>> Cancel(Guid id, CancellationToken ct) =>
        Ok(await enrollmentService.CancelAsync(id, ct));

    /// <summary>
    /// Enrolls participants in workshops from an Excel file with the columns ParticipantEmail and WorkshopCode
    /// (employees only).
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = Roles.Employee)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxImportFileSize)]
    [ProducesResponseType<EnrollmentImportResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EnrollmentImportResult>> Import(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return Problem(title: "Validation error", detail: "An Excel (.xlsx) file is required.",
                statusCode: StatusCodes.Status400BadRequest);

        await using var stream = file.OpenReadStream();
        return Ok(await enrollmentService.ImportAsync(stream, ct));
    }

    /// <summary>Downloads the PDF certificate of an active enrollment (its owner or an employee).</summary>
    [HttpGet("{id:guid}/certificate")]
    [Authorize(Roles = $"{Roles.Participant},{Roles.Employee}")]
    [Produces("application/pdf")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetCertificate(Guid id, CancellationToken ct)
    {
        var certificate = await enrollmentService.GetCertificateAsync(id, ct);
        return File(certificate.Content, certificate.ContentType, certificate.FileName);
    }
}
