using ClosedXML.Excel;
using SkillHub.Application.Abstractions.Services;
using SkillHub.Application.Common.Exceptions;

namespace SkillHub.Infrastructure.Services;

public class ExcelEnrollmentImportReader : IEnrollmentImportReader
{
    private static readonly string[] EmailColumn = ["ParticipantEmail", "Email"];
    private static readonly string[] CodeColumn = ["WorkshopCode", "Code"];

    public IReadOnlyList<EnrollmentImportRow> Read(Stream file)
    {
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(file);
        }
        catch (Exception)
        {
            throw new ValidationException("File", "The file is not a valid Excel (.xlsx) workbook.");
        }

        using (workbook)
        {
            // Accept either a single-sheet file or the full SkillHub_Data workbook.
            var sheet = workbook.Worksheets.FirstOrDefault(s => s.Name.Equals("Enrollments", StringComparison.OrdinalIgnoreCase))
                        ?? workbook.Worksheets.First();

            if (sheet.FirstRowUsed() is null)
                throw new ValidationException("File", "The worksheet is empty.");

            var reader = new ExcelSheetReader(sheet);
            if (!reader.Has(EmailColumn) || !reader.Has(CodeColumn))
                throw new ValidationException("File", "The worksheet must have the columns ParticipantEmail and WorkshopCode.");

            return reader.DataRows
                .Select(row => new EnrollmentImportRow(
                    row.RowNumber(),
                    reader.GetString(row, EmailColumn),
                    reader.GetString(row, CodeColumn)))
                .ToList();
        }
    }
}
