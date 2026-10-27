using System.Globalization;

namespace SkillHub.Web.Infrastructure.Localization;

public static class SupportedCultures
{
    /// <summary>Spanish is the main language; English is available through the language switch.</summary>
    public const string Spanish = "es-CO";
    public const string English = "en-US";

    public static readonly string[] All = [Spanish, English];

    public static bool IsSpanish => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es";
}
