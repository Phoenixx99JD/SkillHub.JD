using SkillHub.Domain.Entities;

namespace SkillHub.Application.Abstractions.Persistence;

public interface IInstructorRepository
{
    Task<Instructor?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Instructor?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<Instructor>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Instructor instructor, CancellationToken ct = default);
}
