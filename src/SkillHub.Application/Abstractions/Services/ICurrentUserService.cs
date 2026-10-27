namespace SkillHub.Application.Abstractions.Services;

/// <summary>Information about the authenticated user of the current request.</summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    string UserId { get; }
    string Email { get; }
    bool IsEmployee { get; }
    bool IsParticipant { get; }
}
