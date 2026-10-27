using SkillHub.Application.Abstractions.Persistence;
using SkillHub.Application.DTOs.Instructors;
using SkillHub.Application.Services.Interfaces;

namespace SkillHub.Application.Services.Implementations;

public class InstructorService(IInstructorRepository instructors) : IInstructorService
{
    public async Task<IReadOnlyList<InstructorDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await instructors.GetAllAsync(ct);
        return list.Select(i => i.ToDto()).ToList();
    }
}
