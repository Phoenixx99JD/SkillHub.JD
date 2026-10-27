using SkillHub.Domain.Common;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Enrollment : Entity
{
    public static readonly TimeSpan ParticipantCancellationWindow = TimeSpan.FromHours(24);

    public Guid WorkshopId { get; private set; }
    public Workshop Workshop { get; private set; } = null!;

    public Guid ParticipantId { get; private set; }
    public Participant Participant { get; private set; } = null!;

    public EnrollmentStatus Status { get; private set; }
    public DateTime EnrolledAt { get; private set; }

    public DateTime? CancelledAt { get; private set; }
    public string? CancelledByUserId { get; private set; }
    public string? CancelledByEmail { get; private set; }

    public bool IsActive => Status == EnrollmentStatus.Active;

    private Enrollment() { }

    internal Enrollment(Workshop workshop, Participant participant, DateTime enrolledAt)
    {
        Workshop = workshop;
        WorkshopId = workshop.Id;
        Participant = participant;
        ParticipantId = participant.Id;
        Status = EnrollmentStatus.Active;
        EnrolledAt = enrolledAt;
    }

    /// <summary>
    /// Cancels the enrollment. Participants must cancel at least 24 hours before the workshop
    /// starts; employees can cancel at any time before it starts.
    /// </summary>
    public void Cancel(string cancelledByUserId, string cancelledByEmail, bool isEmployee, DateTime now)
    {
        if (!IsActive)
            throw new DomainException("The enrollment is already cancelled.");

        if (Workshop.HasStarted(now))
            throw new DomainException("The workshop has already started; the enrollment can no longer be cancelled.");

        if (!isEmployee && Workshop.StartsAt - now < ParticipantCancellationWindow)
            throw new DomainException("Enrollments can only be cancelled up to 24 hours before the workshop starts.");

        Status = EnrollmentStatus.Cancelled;
        CancelledAt = now;
        CancelledByUserId = cancelledByUserId;
        CancelledByEmail = cancelledByEmail;
        Workshop.Touch();
    }
}
