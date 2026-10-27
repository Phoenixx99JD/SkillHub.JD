using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace SkillHub.Web.Infrastructure.Localization;

/// <summary>
/// Gives the validation attributes of the application DTOs (which have no ErrorMessage) a resource key,
/// so data annotations localization translates them. Messages that already have one are left alone.
/// </summary>
public sealed class LocalizedValidationMetadataProvider : IValidationMetadataProvider
{
    // EmailAddressAttribute fills ErrorMessage in its constructor, so its default doesn't count as a custom message.
    private static readonly string? DefaultEmailMessage = new EmailAddressAttribute().ErrorMessage;

    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach (var attribute in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
        {
            var hasCustomMessage = attribute.ErrorMessageResourceName is not null ||
                                   (attribute.ErrorMessage is not null &&
                                    !(attribute is EmailAddressAttribute && attribute.ErrorMessage == DefaultEmailMessage));
            if (hasCustomMessage)
                continue;

            attribute.ErrorMessage = attribute switch
            {
                RequiredAttribute => "Este campo es obligatorio.",
                EmailAddressAttribute => "Introduce un correo electrónico válido.",
                StringLengthAttribute { MinimumLength: > 0 } => "Debe tener entre {2} y {1} caracteres.",
                StringLengthAttribute => "Debe tener como máximo {1} caracteres.",
                _ => null
            };
        }
    }
}
