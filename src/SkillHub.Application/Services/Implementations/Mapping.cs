using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.DTOs.Instructors;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Domain.Entities;

namespace SkillHub.Application.Services.Implementations;

/// <summary>Entity to DTO mappings.</summary>
internal static class Mapping
{
    public static WorkshopDto ToDto(this Workshop w) => new(
        w.Id, w.Code, w.Title, w.Description,
        w.Instructor.FullName, w.Instructor.Email, w.Instructor.Specialty,
        w.StartsAt, w.EndsAt, w.Capacity, w.AvailableSeats);

    public static EnrollmentDto ToDto(this Enrollment e, DateTime now) => new(
        e.Id, e.WorkshopId, e.Workshop.Code, e.Workshop.Title, e.Workshop.Instructor.FullName,
        e.Workshop.StartsAt, e.Workshop.EndsAt, e.Status.ToString(), e.EnrolledAt,
        e.CancelledAt, e.CancelledByEmail,
        CanBeCancelled: e.IsActive && e.Workshop.StartsAt - now >= Enrollment.ParticipantCancellationWindow);

    public static WorkshopEnrollmentDto ToWorkshopEnrollmentDto(this Enrollment e) => new(
        e.Id, e.ParticipantId, e.Participant.FullName, e.Participant.Email, e.Status.ToString(),
        e.EnrolledAt, e.CancelledAt, e.CancelledByEmail);

    public static InstructorDto ToDto(this Instructor i) => new(i.Id, i.FullName, i.Email, i.Specialty);
}
