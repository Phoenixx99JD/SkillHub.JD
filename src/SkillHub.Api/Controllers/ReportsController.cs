using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Common;
using SkillHub.Application.DTOs.Reports;
using SkillHub.Application.Services.Interfaces;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize(Roles = Roles.Employee)]
public class ReportsController(IReportService reportService) : ControllerBase
{
    /// <summary>Workshops with the most active enrollments, highest first.</summary>
    /// <param name="top">Number of workshops to return (1-100).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("top-workshops")]
    public async Task<ActionResult<IReadOnlyList<TopWorkshopDto>>> GetTopWorkshops([FromQuery] int top = 10,
        CancellationToken ct = default) =>
        Ok(await reportService.GetTopWorkshopsAsync(top, ct));
}
