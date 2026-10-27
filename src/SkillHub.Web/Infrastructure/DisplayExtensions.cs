using System.Globalization;

namespace SkillHub.Web.Infrastructure;

public static class DisplayExtensions
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");

    public static string ToDisplayDate(this DateTime value) => value.ToString("ddd, MMM d, yyyy", Culture);

    public static string ToDisplayTime(this DateTime value) => value.ToString("h:mm tt", Culture);

    public static string ToDisplayDateTime(this DateTime value) => value.ToString("MMM d, yyyy h:mm tt", Culture);

    public static string ToSchedule(this DateTime start, DateTime end) =>
        start.Date == end.Date
            ? $"{start.ToDisplayDate()} · {start.ToDisplayTime()} – {end.ToDisplayTime()}"
            : $"{start.ToDisplayDateTime()} – {end.ToDisplayDateTime()}";
}
