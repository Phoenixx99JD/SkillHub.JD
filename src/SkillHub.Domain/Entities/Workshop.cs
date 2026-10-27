using SkillHub.Domain.Common;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Workshop : Entity
{
    private readonly List<Enrollment> _enrollments = [];

    public string Code { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    public Guid InstructorId { get; private set; }
    public Instructor Instructor { get; private set; } = null!;

    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public int Capacity { get; private set; }

    /// <summary>
    /// Concurrency token. It changes every time the set of active enrollments changes, so two
    /// simultaneous enrollments cannot both take the last seat.
    /// </summary>
    public Guid Version { get; private set; } = Guid.NewGuid();

    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();

    private Workshop() { }

    private Workshop(string code, string title, string description, Instructor instructor,
        DateTime startsAt, DateTime endsAt, int capacity)
    {
        Code = code;
        Title = title;
        Description = description;
        Instructor = instructor;
        InstructorId = instructor.Id;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Capacity = capacity;
    }

    /// <summary>Creates a new workshop, enforcing the publishing rules.</summary>
    public static Workshop Create(string code, string title, string description, Instructor instructor,
        DateTime startsAt, DateTime endsAt, int capacity, DateTime now)
    {
        if (startsAt <= now)
            throw new DomainException("The workshop must start in the future.");

        return Restore(code, title, description, instructor, startsAt, endsAt, capacity);
    }

    /// <summary>
    /// Rebuilds an already published workshop (e.g. from seed data) without the "future date" rule.
    /// </summary>
    public static Workshop Restore(string code, string title, string description, Instructor instructor,
        DateTime startsAt, DateTime endsAt, int capacity)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("The workshop code is required.");
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("The workshop title is required.");
        if (endsAt <= startsAt) throw new DomainException("The end time must be later than the start time.");
        if (capacity <= 0) throw new DomainException("The capacity must be greater than zero.");
        ArgumentNullException.ThrowIfNull(instructor);

        return new Workshop(code.Trim().ToUpperInvariant(), title.Trim(), description?.Trim() ?? string.Empty,
            instructor, startsAt, endsAt, capacity);
    }

    public int ActiveEnrollmentsCount => _enrollments.Count(e => e.IsActive);

    public int AvailableSeats => Math.Max(0, Capacity - ActiveEnrollmentsCount);

    public bool HasStarted(DateTime now) => StartsAt <= now;

    public bool OverlapsWith(Workshop other) => StartsAt < other.EndsAt && other.StartsAt < EndsAt;

    /// <summary>
    /// Enrolls a participant applying every enrollment rule.
    /// </summary>
    /// <param name="participant">The participant to enroll.</param>
    /// <param name="participantActiveWorkshops">Workshops (other than this one) where the participant
    /// currently holds an active enrollment. Used to detect schedule overlaps.</param>
    /// <param name="now">Current UTC time.</param>
    public Enrollment Enroll(Participant participant, IEnumerable<Workshop> participantActiveWorkshops, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(participant);

        if (HasStarted(now))
            throw new DomainException($"Workshop {Code} has already started; enrollment is closed.");

        if (_enrollments.Any(e => e.IsActive && e.ParticipantId == participant.Id))
            throw new DomainException($"The participant already has an active enrollment in workshop {Code}.");

        if (ActiveEnrollmentsCount >= Capacity)
            throw new DomainException($"Workshop {Code} has no available seats.");

        var overlapping = participantActiveWorkshops.FirstOrDefault(w => w.Id != Id && OverlapsWith(w));
        if (overlapping is not null)
            throw new DomainException(
                $"Workshop {Code} overlaps with workshop {overlapping.Code}, where the participant is already enrolled.");

        var enrollment = new Enrollment(this, participant, now);
        _enrollments.Add(enrollment);
        Touch();
        return enrollment;
    }

    internal void Touch() => Version = Guid.NewGuid();
}
