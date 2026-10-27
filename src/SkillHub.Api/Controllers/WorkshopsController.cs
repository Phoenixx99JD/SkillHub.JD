using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Common;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Services.Interfaces;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/workshops")]
public class WorkshopsController(IWorkshopService workshopService) : ControllerBase
{
    /// <summary>Lists the workshops that have not started yet, with their available seats.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<WorkshopDto>>> GetUpcoming(CancellationToken ct) =>
        Ok(await workshopService.GetUpcomingAsync(ct));

    /// <summary>Gets a workshop with its available seats.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<WorkshopDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkshopDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await workshopService.GetByIdAsync(id, ct));

    /// <summary>Creates a workshop (employees only).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType<WorkshopDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<WorkshopDto>> Create(CreateWorkshopRequest request, CancellationToken ct)
    {
        var workshop = await workshopService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = workshop.Id }, workshop);
    }

    /// <summary>Lists the enrollments of a workshop (employees only).</summary>
    [HttpGet("{id:guid}/enrollments")]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType<IReadOnlyList<WorkshopEnrollmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<WorkshopEnrollmentDto>>> GetEnrollments(Guid id, CancellationToken ct) =>
        Ok(await workshopService.GetEnrollmentsAsync(id, ct));
}
