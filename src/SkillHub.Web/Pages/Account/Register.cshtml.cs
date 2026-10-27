using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Application.Services.Interfaces;
using SkillHub.Web.Infrastructure;

namespace SkillHub.Web.Pages.Account;

public class RegisterModel(IAuthService authService) : AppPageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public class InputModel : RegisterRequest
    {
        [Required, Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public IActionResult OnGet() =>
        User.Identity?.IsAuthenticated == true ? RedirectToPage("/Index") : Page();

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        var request = new RegisterRequest { FullName = Input.FullName, Email = Input.Email, Password = Input.Password };
        UserDto? user = null;
        if (!await TryExecuteAsync(async () => user = await authService.RegisterAsync(request, ct)))
            return Page();

        await HttpContext.SignInUserAsync(user!);
        SuccessMessage = L["¡Bienvenido a SkillHub, {0}!", user!.FullName];
        return RedirectToLocal(ReturnUrl);
    }
}
