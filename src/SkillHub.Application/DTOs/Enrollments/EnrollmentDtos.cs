using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs.Enrollments;

public class CreateEnrollmentRequest
{
    [Required]
    public Guid WorkshopId { get; set; }
}

/// <summary>An enrollment as seen by its participant.</summary>
public record EnrollmentDto(
    Guid Id,
    Guid WorkshopId,
    string WorkshopCode,
    string WorkshopTitle,
    string InstructorName,
    DateTime WorkshopStartsAt,
    DateTime WorkshopEndsAt,
    string Status,
    DateTime EnrolledAt,
    DateTime? CancelledAt,
    string? CancelledBy,
    bool CanBeCancelled);

/// <summary>An enrollment of a workshop as seen by an employee.</summary>
public record WorkshopEnrollmentDto(
    Guid Id,
    Guid ParticipantId,
    string ParticipantName,
    string ParticipantEmail,
    string Status,
    DateTime EnrolledAt,
    DateTime? CancelledAt,
    string? CancelledBy);

public record ImportRowError(int RowNumber, string Reason);

public record EnrollmentImportResult(int TotalRows, int EnrolledCount, int RejectedCount, IReadOnlyList<ImportRowError> RejectedRows);

public record CertificateFile(string FileName, string ContentType, byte[] Content);
