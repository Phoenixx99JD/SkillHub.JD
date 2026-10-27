using System.ComponentModel.DataAnnotations;
using ValidationException = SkillHub.Application.Common.Exceptions.ValidationException;

namespace SkillHub.Application.Common.Validation;

/// <summary>
/// Validates request DTOs using their data annotations so that every presentation layer
/// (API or Web) gets the same validation.
/// </summary>
public static class RequestValidator
{
    public static void Validate(object request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true))
            return;

        var errors = results
            .SelectMany(r => (r.MemberNames.Any() ? r.MemberNames : [string.Empty])
                .Select(member => (member, message: r.ErrorMessage ?? "Invalid value.")))
            .GroupBy(x => x.member)
            .ToDictionary(g => g.Key, g => g.Select(x => x.message).ToArray());

        throw new ValidationException(errors);
    }
}
