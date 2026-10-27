using SkillHub.Application.DTOs.Auth;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.DTOs.Instructors;
using SkillHub.Application.DTOs.Reports;
using SkillHub.Application.DTOs.Workshops;

namespace SkillHub.Application.Services.Interfaces;

public interface IAuthService
{
    /// <summary>Registers a new participant.</summary>
    Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>Validates the credentials and returns the user (used by cookie based clients).</summary>
    Task<UserDto> AuthenticateAsync(LoginRequest request, CancellationToken ct = default);

    /// <summary>Validates the credentials and issues a JWT.</summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public interface IWorkshopService
{
    Task<IReadOnlyList<WorkshopDto>> GetUpcomingAsync(CancellationToken ct = default);
    Task<WorkshopDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WorkshopDto> CreateAsync(CreateWorkshopRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<WorkshopEnrollmentDto>> GetEnrollmentsAsync(Guid workshopId, CancellationToken ct = default);
}

public interface IEnrollmentService
{
    Task<EnrollmentDto> EnrollCurrentParticipantAsync(CreateEnrollmentRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<EnrollmentDto>> GetMineAsync(CancellationToken ct = default);
    Task<EnrollmentDto> CancelAsync(Guid enrollmentId, CancellationToken ct = default);
    Task<EnrollmentImportResult> ImportAsync(Stream file, CancellationToken ct = default);
    Task<CertificateFile> GetCertificateAsync(Guid enrollmentId, CancellationToken ct = default);
}

public interface IReportService
{
    Task<IReadOnlyList<TopWorkshopDto>> GetTopWorkshopsAsync(int top, CancellationToken ct = default);
}

public interface IInstructorService
{
    Task<IReadOnlyList<InstructorDto>> GetAllAsync(CancellationToken ct = default);
}
