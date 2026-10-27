using System.Globalization;
using ClosedXML.Excel;

namespace SkillHub.Infrastructure.Services;

/// <summary>
/// Reads an Excel worksheet by header names. Headers are matched ignoring case, spaces,
/// underscores and hyphens, and each logical column accepts several aliases.
/// </summary>
internal sealed class ExcelSheetReader
{
    private readonly IXLWorksheet _sheet;
    private readonly Dictionary<string, int> _columns;
    private readonly int _headerRow;

    public ExcelSheetReader(IXLWorksheet sheet)
    {
        _sheet = sheet;
        var header = sheet.FirstRowUsed() ?? throw new InvalidOperationException($"Sheet {sheet.Name} is empty.");
        _headerRow = header.RowNumber();
        _columns = header.CellsUsed()
            .GroupBy(c => Normalize(c.GetString()))
            .ToDictionary(g => g.Key, g => g.First().Address.ColumnNumber);
    }

    public static string Normalize(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    public bool Has(params string[] aliases) => Find(aliases) is not null;

    public IEnumerable<IXLRow> DataRows => _sheet.RowsUsed()
        .Where(r => r.RowNumber() > _headerRow && !r.Cells().All(c => c.IsEmpty()));

    public string? GetString(IXLRow row, params string[] aliases)
    {
        var column = Find(aliases);
        if (column is null) return null;
        var value = row.Cell(column.Value).GetFormattedString().Trim();
        return value.Length == 0 ? null : value;
    }

    public int? GetInt(IXLRow row, params string[] aliases)
    {
        var column = Find(aliases);
        if (column is null) return null;
        var cell = row.Cell(column.Value);
        if (cell.Value.IsNumber) return (int)cell.Value.GetNumber();
        return int.TryParse(cell.GetString().Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
    }

    public DateTime? GetDateTime(IXLRow row, params string[] aliases)
    {
        var column = Find(aliases);
        if (column is null) return null;
        var cell = row.Cell(column.Value);
        if (cell.Value.IsDateTime) return cell.Value.GetDateTime();
        if (cell.Value.IsNumber) return DateTime.FromOADate(cell.Value.GetNumber());
        return ParseDate(cell.GetString());
    }

    public TimeSpan? GetTime(IXLRow row, params string[] aliases)
    {
        var column = Find(aliases);
        if (column is null) return null;
        var cell = row.Cell(column.Value);
        if (cell.Value.IsTimeSpan) return cell.Value.GetTimeSpan();
        if (cell.Value.IsDateTime) return cell.Value.GetDateTime().TimeOfDay;
        if (cell.Value.IsNumber) return TimeSpan.FromDays(cell.Value.GetNumber() % 1);
        var text = cell.GetString().Trim();
        if (TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var ts)) return ts;
        return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt.TimeOfDay : null;
    }

    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd",
        "dd/MM/yyyy HH:mm", "dd/MM/yyyy H:mm", "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy", "d/M/yyyy H:mm", "d/M/yyyy"
    ];

    private static DateTime? ParseDate(string text)
    {
        text = text.Trim();
        if (DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
            return exact;
        return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null;
    }

    private int? Find(string[] aliases)
    {
        foreach (var alias in aliases)
            if (_columns.TryGetValue(Normalize(alias), out var column))
                return column;
        return null;
    }
}
