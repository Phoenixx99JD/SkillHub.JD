namespace SkillHub.Application.Abstractions.Services;

public record AccessToken(string Token, DateTime ExpiresAtUtc);

public interface ITokenService
{
    AccessToken CreateToken(string userId, string email, string fullName, IEnumerable<string> roles);
}
