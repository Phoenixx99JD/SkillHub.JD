using System.Text.RegularExpressions;

namespace SkillHub.Web.Infrastructure.Localization;

/// <summary>
/// The domain, application and Identity messages are written in English because the API shares them.
/// This translates the ones a participant can see in the web app into Spanish when Spanish is the current
/// language. Unknown messages are shown as they are.
/// </summary>
public static class ErrorMessageTranslator
{
    private static readonly (Regex Pattern, string Spanish)[] Rules =
    [
        // Authentication and registration
        (Exact("Invalid email or password."), "Correo o contraseña incorrectos."),
        (Exact("Authentication is required."), "Debes iniciar sesión."),
        (Pattern(@"The email (.+) is already registered\."), "El correo {0} ya está registrado."),
        (Pattern(@"Passwords must be at least (\d+) characters\."), "La contraseña debe tener al menos {0} caracteres."),
        (Exact("Passwords must have at least one uppercase ('A'-'Z')."), "La contraseña debe tener al menos una letra mayúscula."),
        (Exact("Passwords must have at least one lowercase ('a'-'z')."), "La contraseña debe tener al menos una letra minúscula."),
        (Exact("Passwords must have at least one digit ('0'-'9')."), "La contraseña debe tener al menos un número."),
        (Pattern(@"Passwords must use at least (\d+) different characters\."), "La contraseña debe usar al menos {0} caracteres distintos."),
        (Pattern(@"(?:Username|Email) '(.+)' is already taken\."), "El correo {0} ya está registrado."),

        // Enrollment rules
        (Pattern(@"Workshop (\S+) has already started; enrollment is closed\."), "El taller {0} ya comenzó; las inscripciones están cerradas."),
        (Pattern(@"The participant already has an active enrollment in workshop (\S+)\."), "Ya tienes una inscripción activa en el taller {0}."),
        (Pattern(@"Workshop (\S+) has no available seats\."), "El taller {0} no tiene cupos disponibles."),
        (Pattern(@"Workshop (\S+) overlaps with workshop (\S+), where the participant is already enrolled\."),
            "El taller {0} se cruza en horario con el taller {1}, en el que ya estás inscrito."),
        (Exact("The enrollment is already cancelled."), "La inscripción ya está cancelada."),
        (Exact("The workshop has already started; the enrollment can no longer be cancelled."),
            "El taller ya comenzó; la inscripción ya no se puede cancelar."),
        (Exact("Enrollments can only be cancelled up to 24 hours before the workshop starts."),
            "Las inscripciones solo se pueden cancelar hasta 24 horas antes del inicio del taller."),
        (Exact("Certificates are only available for active enrollments."), "Los certificados solo están disponibles para inscripciones activas."),
        (Exact("Only participants can perform this operation."), "Solo los participantes pueden realizar esta operación."),
        (Exact("You can only access your own enrollments."), "Solo puedes acceder a tus propias inscripciones."),
        (Exact("The participant profile of the current user was not found."), "No se encontró tu perfil de participante."),
        (Pattern(@"Enrollment (\S+) was not found\."), "No se encontró la inscripción."),
        (Pattern(@"Workshop (\S+) was not found\."), "No se encontró el taller."),

        // Persistence
        (Exact("The data was modified by another request. Please try again."), "Otra persona modificó los datos al mismo tiempo. Inténtalo de nuevo."),
        (Exact("The record already exists."), "El registro ya existe."),

        // Default data annotations messages produced by the application layer validation
        (Pattern(@"The (\w+) field is required\."), "El campo {0} es obligatorio."),
        (Pattern(@"The (\w+) field is not a valid e-mail address\."), "El campo {0} no es un correo electrónico válido."),
        (Pattern(@"The field (\w+) must be a string with a minimum length of (\d+) and a maximum length of (\d+)\."),
            "El campo {0} debe tener entre {1} y {2} caracteres."),
    ];

    public static string Translate(string message)
    {
        if (!SupportedCultures.IsSpanish) return message;

        foreach (var (pattern, spanish) in Rules)
        {
            var match = pattern.Match(message);
            if (match.Success)
                return string.Format(spanish, match.Groups.Values.Skip(1).Select(g => (object)g.Value).ToArray());
        }

        return message;
    }

    private static Regex Exact(string message) => Pattern(Regex.Escape(message));

    private static Regex Pattern(string pattern) => new($"^{pattern}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
}
