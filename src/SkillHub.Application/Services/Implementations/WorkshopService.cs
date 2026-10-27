using SkillHub.Application.Abstractions.Persistence;
using SkillHub.Application.Abstractions.Services;
using SkillHub.Application.Common.Exceptions;
using SkillHub.Application.Common.Validation;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Services.Interfaces;
using SkillHub.Domain.Entities;

namespace SkillHub.Application.Services.Implementations;

public class WorkshopService(
    IWorkshopRepository workshops,
    IEnrollmentRepository enrollments,
    IInstructorRepository instructors,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : IWorkshopService
{
    public async Task<IReadOnlyList<WorkshopDto>> GetUpcomingAsync(CancellationToken ct = default)
    {
        var upcoming = await workshops.GetUpcomingAsync(clock.Now, ct);
        return upcoming.Select(w => w.ToDto()).ToList();
    }

    public async Task<WorkshopDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var workshop = await workshops.GetByIdAsync(id, ct)
                       ?? throw new NotFoundException($"Workshop {id} was not found.");
        return workshop.ToDto();
    }

    public async Task<WorkshopDto> CreateAsync(CreateWorkshopRequest request, CancellationToken ct = default)
    {
        RequestValidator.Validate(request);

        var code = request.Code.Trim().ToUpperInvariant();
        if (await workshops.ExistsByCodeAsync(code, ct))
            throw new ConflictException($"A workshop with code {code} already exists.");

        var instructor = await instructors.GetByEmailAsync(request.InstructorEmail.Trim(), ct)
                         ?? throw new NotFoundException($"No instructor is registered with email {request.InstructorEmail}.");

        var workshop = Workshop.Create(code, request.Title, request.Description, instructor,
            request.StartsAt, request.EndsAt, request.Capacity, clock.Now);

        await workshops.AddAsync(workshop, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return workshop.ToDto();
    }

    public async Task<IReadOnlyList<WorkshopEnrollmentDto>> GetEnrollmentsAsync(Guid workshopId, CancellationToken ct = default)
    {
        if (await workshops.GetByIdAsync(workshopId, ct) is null)
            throw new NotFoundException($"Workshop {workshopId} was not found.");

        var list = await enrollments.GetByWorkshopAsync(workshopId, ct);
        return list.Select(e => e.ToWorkshopEnrollmentDto()).ToList();
    }
}
