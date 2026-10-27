using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using SkillHub.Application.Common.Exceptions;
using SkillHub.Domain.Exceptions;
using SkillHub.Web.Infrastructure.Localization;

namespace SkillHub.Web.Infrastructure;

/// <summary>Base page with helpers to show the result of application operations.</summary>
public abstract class AppPageModel : PageModel
{
    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ErrorMessage { get; set; }

    /// <summary>Shared UI strings, in the language of the current request.</summary>
    protected IStringLocalizer L => HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();

    /// <summary>
    /// Runs an operation and turns business, validation and authorization errors into model errors
    /// instead of an error page. Pages that redirect afterwards should call <see cref="KeepErrorsForRedirect"/>.
    /// </summary>
    protected async Task<bool> TryExecuteAsync(Func<Task> operation)
    {
        try
        {
            await operation();
            return true;
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors.SelectMany(e => e.Value))
                ModelState.AddModelError(string.Empty, ErrorMessageTranslator.Translate(error));
        }
        catch (Exception ex) when (ex is DomainException or AppException)
        {
            ModelState.AddModelError(string.Empty, ErrorMessageTranslator.Translate(ex.Message));
        }

        return false;
    }

    /// <summary>Moves the current model errors to TempData so they survive a redirect.</summary>
    protected void KeepErrorsForRedirect() =>
        ErrorMessage = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

    protected IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToPage("/Index");
}
