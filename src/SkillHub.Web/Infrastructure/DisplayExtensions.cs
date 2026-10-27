using System.Globalization;
using SkillHub.Web.Infrastructure.Localization;

namespace SkillHub.Web.Infrastructure;

/// <summary>Date formatting in the language of the current request (Spanish by default).</summary>
public static class DisplayExtensions
{
    private static CultureInfo Culture => CultureInfo.CurrentCulture;

    public static string ToDisplayDate(this DateTime value) =>
        SupportedCultures.IsSpanish
            ? Capitalize(value.ToString("dddd, d 'de' MMMM 'de' yyyy", Culture))
            : value.ToString("ddd, MMM d, yyyy", Culture);

    public static string ToDisplayTime(this DateTime value) => value.ToString("h:mm tt", Culture);

    public static string ToDisplayDateTime(this DateTime value) =>
        SupportedCultures.IsSpanish
            ? value.ToString("d 'de' MMM 'de' yyyy, h:mm tt", Culture)
            : value.ToString("MMM d, yyyy h:mm tt", Culture);

    /// <summary>"DIC" / "DEC", for the calendar tile.</summary>
    public static string ToMonthAbbreviation(this DateTime value) =>
        value.ToString("MMM", Culture).TrimEnd('.').ToUpper(Culture);

    public static string ToTimeRange(this DateTime start, DateTime end) => $"{start.ToDisplayTime()} – {end.ToDisplayTime()}";

    /// <summary>"Andrés Mejía" → "AM".</summary>
    public static string ToInitials(this string name) =>
        string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpperInvariant(p[0])));

    public static string ToSchedule(this DateTime start, DateTime end) =>
        start.Date == end.Date
            ? $"{start.ToDisplayDate()} · {start.ToTimeRange(end)}"
            : $"{start.ToDisplayDateTime()} – {end.ToDisplayDateTime()}";

    private static string Capitalize(string text) =>
        text.Length == 0 ? text : char.ToUpper(text[0], Culture) + text[1..];
}
