using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Common;
using SkillHub.Application.DTOs.Instructors;
using SkillHub.Application.Services.Interfaces;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/instructors")]
[Authorize(Roles = Roles.Employee)]
public class InstructorsController(IInstructorService instructorService) : ControllerBase
{
    /// <summary>Lists the registered instructors (useful to create workshops).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InstructorDto>>> GetAll(CancellationToken ct) =>
        Ok(await instructorService.GetAllAsync(ct));
}
