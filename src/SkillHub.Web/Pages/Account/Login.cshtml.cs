using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Common;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Application.Services.Interfaces;
using SkillHub.Web.Infrastructure;

namespace SkillHub.Web.Pages.Account;

public class LoginModel(IAuthService authService) : AppPageModel
{
    [BindProperty]
    public LoginRequest Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet() =>
        User.Identity?.IsAuthenticated == true ? RedirectToPage("/Index") : Page();

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        UserDto? user = null;
        if (!await TryExecuteAsync(async () => user = await authService.AuthenticateAsync(Input, ct)))
            return Page();

        // This application is for participants; employees work through the API.
        if (!user!.Roles.Contains(Roles.Participant))
        {
            ModelState.AddModelError(string.Empty,
                "This portal is for participants. Employees manage workshops through the SkillHub API.");
            return Page();
        }

        await HttpContext.SignInUserAsync(user);
        return RedirectToLocal(ReturnUrl);
    }
}
