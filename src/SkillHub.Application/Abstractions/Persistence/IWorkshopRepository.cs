using SkillHub.Domain.Entities;

namespace SkillHub.Application.Abstractions.Persistence;

public interface IWorkshopRepository
{
    /// <summary>Gets a workshop with its instructor and its active enrollments.</summary>
    Task<Workshop?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Gets a workshop by code with its instructor and its active enrollments.</summary>
    Task<Workshop?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>Workshops that have not started yet, with their active enrollments.</summary>
    Task<IReadOnlyList<Workshop>> GetUpcomingAsync(DateTime now, CancellationToken ct = default);

    /// <summary>Workshops where the participant has an active enrollment.</summary>
    Task<IReadOnlyList<Workshop>> GetWithActiveEnrollmentForParticipantAsync(Guid participantId, CancellationToken ct = default);

    /// <summary>Workshops ordered by number of active enrollments, highest first.</summary>
    Task<IReadOnlyList<(Workshop Workshop, int ActiveEnrollments)>> GetTopByActiveEnrollmentsAsync(int top, CancellationToken ct = default);

    Task AddAsync(Workshop workshop, CancellationToken ct = default);
}
