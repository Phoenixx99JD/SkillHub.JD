namespace SkillHub.Application.Abstractions.Services;

public record EnrollmentCertificateData(
    Guid EnrollmentId,
    string ParticipantName,
    string ParticipantEmail,
    string WorkshopCode,
    string WorkshopTitle,
    string InstructorName,
    DateTime WorkshopStartsAt,
    DateTime WorkshopEndsAt,
    DateTime EnrolledAt,
    DateTime GeneratedAt);

/// <summary>Generates the PDF enrollment certificate.</summary>
public interface ICertificateGenerator
{
    byte[] Generate(EnrollmentCertificateData data);
}
