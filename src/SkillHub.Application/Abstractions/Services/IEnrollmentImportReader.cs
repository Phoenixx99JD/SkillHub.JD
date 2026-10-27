namespace SkillHub.Application.Abstractions.Services;

public record EnrollmentImportRow(int RowNumber, string? ParticipantEmail, string? WorkshopCode);

/// <summary>Reads the rows of an enrollment import file (Excel).</summary>
public interface IEnrollmentImportReader
{
    /// <exception cref="Common.Exceptions.ValidationException">The file cannot be read or lacks the required columns.</exception>
    IReadOnlyList<EnrollmentImportRow> Read(Stream file);
}
