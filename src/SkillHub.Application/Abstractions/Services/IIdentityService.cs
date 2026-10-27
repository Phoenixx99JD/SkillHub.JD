namespace SkillHub.Application.Abstractions.Services;

public record AuthenticatedUser(string UserId, string Email, IReadOnlyList<string> Roles);

/// <summary>Abstraction over the user store (ASP.NET Core Identity).</summary>
public interface IIdentityService
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);

    /// <summary>Creates a user with the given role and returns its id.</summary>
    Task<string> CreateUserAsync(string email, string password, string role, CancellationToken ct = default);

    /// <summary>Returns the user when the credentials are valid; otherwise null.</summary>
    Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);
}
