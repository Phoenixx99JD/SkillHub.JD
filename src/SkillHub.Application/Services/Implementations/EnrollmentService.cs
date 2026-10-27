using SkillHub.Application.Abstractions.Persistence;
using SkillHub.Application.Abstractions.Services;
using SkillHub.Application.Common.Exceptions;
using SkillHub.Application.Common.Validation;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Services.Interfaces;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Application.Services.Implementations;

public class EnrollmentService(
    IEnrollmentRepository enrollments,
    IWorkshopRepository workshops,
    IParticipantRepository participants,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IDateTimeProvider clock,
    IEnrollmentImportReader importReader,
    ICertificateGenerator certificateGenerator) : IEnrollmentService
{
    public async Task<EnrollmentDto> EnrollCurrentParticipantAsync(CreateEnrollmentRequest request, CancellationToken ct = default)
    {
        RequestValidator.Validate(request);
        var participant = await GetCurrentParticipantAsync(ct);

        var workshop = await workshops.GetByIdAsync(request.WorkshopId, ct)
                       ?? throw new NotFoundException($"Workshop {request.WorkshopId} was not found.");

        var enrollment = await EnrollAsync(participant, workshop, ct);
        return enrollment.ToDto(clock.Now);
    }

    public async Task<IReadOnlyList<EnrollmentDto>> GetMineAsync(CancellationToken ct = default)
    {
        var participant = await GetCurrentParticipantAsync(ct);
        var now = clock.Now;
        var list = await enrollments.GetByParticipantAsync(participant.Id, ct);
        return list.Select(e => e.ToDto(now)).ToList();
    }

    public async Task<EnrollmentDto> CancelAsync(Guid enrollmentId, CancellationToken ct = default)
    {
        var enrollment = await GetAccessibleEnrollmentAsync(enrollmentId, ct);
        var now = clock.Now;

        enrollment.Cancel(currentUser.UserId, currentUser.Email, currentUser.IsEmployee, now);
        await unitOfWork.SaveChangesAsync(ct);

        return enrollment.ToDto(now);
    }

    public async Task<EnrollmentImportResult> ImportAsync(Stream file, CancellationToken ct = default)
    {
        var rows = importReader.Read(file);
        var rejected = new List<ImportRowError>();
        var enrolled = 0;

        foreach (var row in rows)
        {
            try
            {
                await ImportRowAsync(row, ct);
                enrolled++;
            }
            catch (Exception ex) when (ex is DomainException or AppException)
            {
                // Valid rows are kept even when others fail: each row is saved on its own.
                unitOfWork.DiscardChanges();
                rejected.Add(new ImportRowError(row.RowNumber, ex.Message));
            }
        }

        return new EnrollmentImportResult(rows.Count, enrolled, rejected.Count, rejected);
    }

    public async Task<CertificateFile> GetCertificateAsync(Guid enrollmentId, CancellationToken ct = default)
    {
        var enrollment = await GetAccessibleEnrollmentAsync(enrollmentId, ct);

        if (!enrollment.IsActive)
            throw new DomainException("Certificates are only available for active enrollments.");

        var workshop = enrollment.Workshop;
        var pdf = certificateGenerator.Generate(new EnrollmentCertificateData(
            enrollment.Id,
            enrollment.Participant.FullName,
            enrollment.Participant.Email,
            workshop.Code,
            workshop.Title,
            workshop.Instructor.FullName,
            workshop.StartsAt,
            workshop.EndsAt,
            enrollment.EnrolledAt,
            clock.Now));

        return new CertificateFile($"certificate-{workshop.Code}-{enrollment.Id:N}.pdf", "application/pdf", pdf);
    }

    private async Task ImportRowAsync(EnrollmentImportRow row, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(row.ParticipantEmail))
            throw new ValidationException("ParticipantEmail", "ParticipantEmail is empty.");
        if (string.IsNullOrWhiteSpace(row.WorkshopCode))
            throw new ValidationException("WorkshopCode", "WorkshopCode is empty.");

        var participant = await participants.GetByEmailAsync(row.ParticipantEmail.Trim(), ct)
                          ?? throw new NotFoundException($"Participant {row.ParticipantEmail.Trim()} does not exist.");

        var workshop = await workshops.GetByCodeAsync(row.WorkshopCode.Trim(), ct)
                       ?? throw new NotFoundException($"Workshop {row.WorkshopCode.Trim()} does not exist.");

        await EnrollAsync(participant, workshop, ct);
    }

    private async Task<Enrollment> EnrollAsync(Participant participant, Workshop workshop, CancellationToken ct)
    {
        var activeWorkshops = await workshops.GetWithActiveEnrollmentForParticipantAsync(participant.Id, ct);
        var enrollment = workshop.Enroll(participant, activeWorkshops, clock.Now);
        await unitOfWork.SaveChangesAsync(ct);
        return enrollment;
    }

    private async Task<Participant> GetCurrentParticipantAsync(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !currentUser.IsParticipant)
            throw new ForbiddenException("Only participants can perform this operation.");

        return await participants.GetByUserIdAsync(currentUser.UserId, ct)
               ?? throw new NotFoundException("The participant profile of the current user was not found.");
    }

    /// <summary>
    /// Loads an enrollment checking that the current user is its owner or an employee.
    /// </summary>
    private async Task<Enrollment> GetAccessibleEnrollmentAsync(Guid enrollmentId, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException("Authentication is required.");

        var enrollment = await enrollments.GetByIdAsync(enrollmentId, ct)
                         ?? throw new NotFoundException($"Enrollment {enrollmentId} was not found.");

        if (!currentUser.IsEmployee && enrollment.Participant.UserId != currentUser.UserId)
            throw new ForbiddenException("You can only access your own enrollments.");

        return enrollment;
    }
}
