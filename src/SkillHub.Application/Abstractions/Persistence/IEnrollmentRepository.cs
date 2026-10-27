using SkillHub.Domain.Entities;

namespace SkillHub.Application.Abstractions.Persistence;

public interface IEnrollmentRepository
{
    /// <summary>Gets an enrollment with its workshop (and instructor) and participant.</summary>
    Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Enrollment>> GetByParticipantAsync(Guid participantId, CancellationToken ct = default);

    Task<IReadOnlyList<Enrollment>> GetByWorkshopAsync(Guid workshopId, CancellationToken ct = default);
}
