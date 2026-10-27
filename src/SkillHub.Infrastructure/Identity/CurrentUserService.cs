using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SkillHub.Application.Abstractions.Services;
using SkillHub.Application.Common;

namespace SkillHub.Infrastructure.Identity;

/// <summary>Reads the current user from the claims of the HTTP request (JWT or cookie).</summary>
public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public string UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    public string Email => User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    public bool IsEmployee => User?.IsInRole(Roles.Employee) == true;
    public bool IsParticipant => User?.IsInRole(Roles.Participant) == true;
}
