using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required, StringLength(150, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public record UserDto(string UserId, string FullName, string Email, IReadOnlyList<string> Roles);

public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, UserDto User);
