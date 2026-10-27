using Microsoft.AspNetCore.Identity;
using SkillHub.Application.Abstractions.Services;
using SkillHub.Application.Common.Exceptions;

namespace SkillHub.Infrastructure.Identity;

public class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        await userManager.FindByEmailAsync(email) is not null;

    public async Task<string> CreateUserAsync(string email, string password, string role, CancellationToken ct = default)
    {
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Password"] = result.Errors.Select(e => e.Description).ToArray()
            });

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException($"Could not assign role {role}: " +
                                                string.Join(", ", roleResult.Errors.Select(e => e.Description)));

        return user.Id;
    }

    public async Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || await userManager.IsLockedOutAsync(user))
            return null;

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(user);
        var roles = await userManager.GetRolesAsync(user);
        return new AuthenticatedUser(user.Id, user.Email!, roles.ToList());
    }
}
