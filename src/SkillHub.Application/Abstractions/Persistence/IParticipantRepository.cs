using SkillHub.Domain.Entities;

namespace SkillHub.Application.Abstractions.Persistence;

public interface IParticipantRepository
{
    Task<Participant?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Participant?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task AddAsync(Participant participant, CancellationToken ct = default);
}
