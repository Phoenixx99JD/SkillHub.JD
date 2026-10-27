using Microsoft.AspNetCore.Html;

namespace SkillHub.Web.Infrastructure;

/// <summary>
/// Visual category of a workshop, derived from the prefix of its code (FOT-001 → Fotografía).
/// <see cref="Name"/> is a key of the shared UI strings.
/// </summary>
public sealed record WorkshopCategory(string Name, string CssClass, string IconName)
{
    private static readonly Dictionary<string, WorkshopCategory> ByPrefix = new(StringComparer.OrdinalIgnoreCase)
    {
        ["FOT"] = new("Fotografía", "cat-photo", "camera"),
        ["COC"] = new("Cocina", "cat-cooking", "chef"),
        ["ORA"] = new("Oratoria", "cat-speaking", "mic"),
        ["DIS"] = new("Diseño", "cat-design", "palette"),
        ["MKT"] = new("Marketing", "cat-marketing", "megaphone"),
        ["MUS"] = new("Música", "cat-music", "music"),
    };

    private static readonly WorkshopCategory Default = new("Taller", "cat-default", "sparkles");

    public IHtmlContent Icon => Icons.Get(IconName);

    public static WorkshopCategory FromCode(string code)
    {
        var prefix = code.Split('-', 2)[0];
        return ByPrefix.GetValueOrDefault(prefix, Default);
    }
}

/// <summary>Inline SVG icons (Lucide style, stroke based) so the UI doesn't depend on an icon font.</summary>
public static class Icons
{
    private static readonly Dictionary<string, string> Paths = new()
    {
        ["camera"] = """<path d="M14.5 4h-5L7 7H4a2 2 0 0 0-2 2v9a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2h-3l-2.5-3z"/><circle cx="12" cy="13" r="3"/>""",
        ["chef"] = """<path d="M6 13.87A4 4 0 0 1 7.41 6a5.11 5.11 0 0 1 1.05-1.54 5 5 0 0 1 7.08 0A5.11 5.11 0 0 1 16.59 6 4 4 0 0 1 18 13.87V21H6Z"/><line x1="6" x2="18" y1="17" y2="17"/>""",
        ["mic"] = """<path d="M12 2a3 3 0 0 0-3 3v7a3 3 0 0 0 6 0V5a3 3 0 0 0-3-3Z"/><path d="M19 10v2a7 7 0 0 1-14 0v-2"/><line x1="12" x2="12" y1="19" y2="22"/>""",
        ["palette"] = """<circle cx="13.5" cy="6.5" r=".5" fill="currentColor"/><circle cx="17.5" cy="10.5" r=".5" fill="currentColor"/><circle cx="8.5" cy="7.5" r=".5" fill="currentColor"/><circle cx="6.5" cy="12.5" r=".5" fill="currentColor"/><path d="M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10c.93 0 1.65-.75 1.65-1.69 0-.44-.18-.84-.44-1.13-.29-.29-.44-.65-.44-1.13a1.64 1.64 0 0 1 1.67-1.67h2c3.05 0 5.56-2.5 5.56-5.55C21.97 6.01 17.46 2 12 2z"/>""",
        ["megaphone"] = """<path d="m3 11 18-5v12L3 14v-3z"/><path d="M11.6 16.8a3 3 0 1 1-5.8-1.6"/>""",
        ["music"] = """<path d="M9 18V5l12-2v13"/><circle cx="6" cy="18" r="3"/><circle cx="18" cy="16" r="3"/>""",
        ["globe"] = """<circle cx="12" cy="12" r="10"/><path d="M12 2a14.5 14.5 0 0 0 0 20 14.5 14.5 0 0 0 0-20"/><path d="M2 12h20"/>""",
        ["sparkles"] = """<path d="m12 3-1.9 5.8a2 2 0 0 1-1.3 1.3L3 12l5.8 1.9a2 2 0 0 1 1.3 1.3L12 21l1.9-5.8a2 2 0 0 1 1.3-1.3L21 12l-5.8-1.9a2 2 0 0 1-1.3-1.3Z"/>""",
        ["calendar"] = """<rect width="18" height="18" x="3" y="4" rx="2"/><line x1="16" x2="16" y1="2" y2="6"/><line x1="8" x2="8" y1="2" y2="6"/><line x1="3" x2="21" y1="10" y2="10"/>""",
        ["clock"] = """<circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>""",
        ["users"] = """<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>""",
        ["user"] = """<path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/>""",
        ["search"] = """<circle cx="11" cy="11" r="8"/><path d="m21 21-4.3-4.3"/>""",
        ["check"] = """<path d="M20 6 9 17l-5-5"/>""",
        ["download"] = """<path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" x2="12" y1="15" y2="3"/>""",
        ["x"] = """<path d="M18 6 6 18"/><path d="m6 6 12 12"/>""",
        ["arrow-right"] = """<path d="M5 12h14"/><path d="m12 5 7 7-7 7"/>""",
        ["arrow-left"] = """<path d="m12 19-7-7 7-7"/><path d="M19 12H5"/>""",
        ["logout"] = """<path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" x2="9" y1="12" y2="12"/>""",
        ["ticket"] = """<path d="M2 9a3 3 0 0 1 0 6v2a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-2a3 3 0 0 1 0-6V7a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2Z"/><path d="M13 5v2"/><path d="M13 17v2"/><path d="M13 11v2"/>""",
        ["lock"] = """<rect width="18" height="11" x="3" y="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/>""",
        ["map-pin"] = """<path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/><circle cx="12" cy="10" r="3"/>""",
        ["award"] = """<circle cx="12" cy="8" r="6"/><path d="M15.477 12.89 17 22l-5-3-5 3 1.523-9.11"/>""",
    };

    public static IHtmlContent Get(string name, string cssClass = "icon") =>
        new HtmlString(
            $"""<svg class="{cssClass}" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">{Paths[name]}</svg>""");
}
