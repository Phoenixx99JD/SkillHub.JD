using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Abstractions.Persistence;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;

namespace SkillHub.Infrastructure.Persistence.Repositories;

public class WorkshopRepository(SkillHubDbContext db) : IWorkshopRepository
{
    // Only active enrollments are loaded: they are the ones that matter for every rule.
    private IQueryable<Workshop> WithActiveEnrollments => db.Workshops
        .Include(w => w.Instructor)
        .Include(w => w.Enrollments.Where(e => e.Status == EnrollmentStatus.Active));

    public Task<Workshop?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        WithActiveEnrollments.FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<Workshop?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return WithActiveEnrollments.FirstOrDefaultAsync(w => w.Code == normalized, ct);
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return db.Workshops.AnyAsync(w => w.Code == normalized, ct);
    }

    public async Task<IReadOnlyList<Workshop>> GetUpcomingAsync(DateTime now, CancellationToken ct = default) =>
        await WithActiveEnrollments.AsNoTracking()
            .Where(w => w.StartsAt > now)
            .OrderBy(w => w.StartsAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Workshop>> GetWithActiveEnrollmentForParticipantAsync(Guid participantId, CancellationToken ct = default) =>
        await db.Enrollments
            .Where(e => e.ParticipantId == participantId && e.Status == EnrollmentStatus.Active)
            .Select(e => e.Workshop)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<(Workshop Workshop, int ActiveEnrollments)>> GetTopByActiveEnrollmentsAsync(int top, CancellationToken ct = default)
    {
        var rows = await db.Workshops
            .AsNoTracking()
            .Include(w => w.Instructor)
            .Select(w => new
            {
                Workshop = w,
                Active = w.Enrollments.Count(e => e.Status == EnrollmentStatus.Active)
            })
            .OrderByDescending(x => x.Active)
            .ThenBy(x => x.Workshop.StartsAt)
            .Take(top)
            .ToListAsync(ct);

        return rows.Select(x => (x.Workshop, x.Active)).ToList();
    }

    public async Task AddAsync(Workshop workshop, CancellationToken ct = default) =>
        await db.Workshops.AddAsync(workshop, ct);
}

public class EnrollmentRepository(SkillHubDbContext db) : IEnrollmentRepository
{
    private IQueryable<Enrollment> WithDetails => db.Enrollments
        .Include(e => e.Participant)
        .Include(e => e.Workshop).ThenInclude(w => w.Instructor);

    public Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        WithDetails.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Enrollment>> GetByParticipantAsync(Guid participantId, CancellationToken ct = default) =>
        await WithDetails.AsNoTracking()
            .Where(e => e.ParticipantId == participantId)
            .OrderByDescending(e => e.Workshop.StartsAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Enrollment>> GetByWorkshopAsync(Guid workshopId, CancellationToken ct = default) =>
        await WithDetails.AsNoTracking()
            .Where(e => e.WorkshopId == workshopId)
            .OrderBy(e => e.Status)
            .ThenBy(e => e.Participant.FullName)
            .ToListAsync(ct);
}

public class InstructorRepository(SkillHubDbContext db) : IInstructorRepository
{
    public Task<Instructor?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return db.Instructors.FirstOrDefaultAsync(i => i.Email == normalized, ct);
    }

    public Task<Instructor?> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        db.Instructors.FirstOrDefaultAsync(i => i.UserId == userId, ct);

    public async Task<IReadOnlyList<Instructor>> GetAllAsync(CancellationToken ct = default) =>
        await db.Instructors.AsNoTracking().OrderBy(i => i.FullName).ToListAsync(ct);

    public async Task AddAsync(Instructor instructor, CancellationToken ct = default) =>
        await db.Instructors.AddAsync(instructor, ct);
}

public class ParticipantRepository(SkillHubDbContext db) : IParticipantRepository
{
    public Task<Participant?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return db.Participants.FirstOrDefaultAsync(p => p.Email == normalized, ct);
    }

    public Task<Participant?> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        db.Participants.FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public async Task AddAsync(Participant participant, CancellationToken ct = default) =>
        await db.Participants.AddAsync(participant, ct);
}
