using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Tests.Domain;

public class EnrollmentTests
{
    private static readonly DateTime Now = new(2026, 10, 1, 8, 0, 0);
    private static readonly Instructor Instructor = new("Ana Torres", "ana@skillhub.com", "Photography", "user-ana");

    private static (Workshop Workshop, Participant Participant, Enrollment Enrollment) Enrolled(DateTime startsAt, int capacity = 1)
    {
        var workshop = Workshop.Create("FOT-001", "T", "D", Instructor, startsAt, startsAt.AddHours(2), capacity, Now);
        var participant = new Participant("Juan", "juan@mail.com", "user-juan");
        return (workshop, participant, workshop.Enroll(participant, [], Now));
    }

    [Fact]
    public void Participant_CanCancel_MoreThan24HoursBefore_AndSeatIsReleased()
    {
        var (workshop, _, enrollment) = Enrolled(Now.AddDays(3));

        enrollment.Cancel("user-juan", "juan@mail.com", isEmployee: false, Now);

        Assert.Equal(EnrollmentStatus.Cancelled, enrollment.Status);
        Assert.Equal(Now, enrollment.CancelledAt);
        Assert.Equal("user-juan", enrollment.CancelledByUserId);
        Assert.Equal("juan@mail.com", enrollment.CancelledByEmail);
        Assert.Equal(1, workshop.AvailableSeats);
    }

    [Fact]
    public void Participant_CannotCancel_Within24Hours()
    {
        var (_, _, enrollment) = Enrolled(Now.AddHours(23));
        var ex = Assert.Throws<DomainException>(() => enrollment.Cancel("user-juan", "juan@mail.com", false, Now));
        Assert.Contains("24 hours", ex.Message);
    }

    [Fact]
    public void Employee_CanCancel_Within24Hours()
    {
        var (_, _, enrollment) = Enrolled(Now.AddHours(1));
        enrollment.Cancel("user-emp", "emp@skillhub.com", isEmployee: true, Now);
        Assert.Equal(EnrollmentStatus.Cancelled, enrollment.Status);
        Assert.Equal("emp@skillhub.com", enrollment.CancelledByEmail);
    }

    [Fact]
    public void Employee_CannotCancel_AfterWorkshopStarted()
    {
        var (workshop, _, enrollment) = Enrolled(Now.AddHours(1));
        Assert.Throws<DomainException>(() => enrollment.Cancel("user-emp", "emp@skillhub.com", true, workshop.StartsAt));
    }

    [Fact]
    public void Cancel_Twice_Throws()
    {
        var (_, _, enrollment) = Enrolled(Now.AddDays(3));
        enrollment.Cancel("user-juan", "juan@mail.com", false, Now);
        Assert.Throws<DomainException>(() => enrollment.Cancel("user-juan", "juan@mail.com", false, Now));
    }

    [Fact]
    public void Participant_CanReenroll_AfterCancelling()
    {
        var (workshop, participant, enrollment) = Enrolled(Now.AddDays(3));
        enrollment.Cancel("user-juan", "juan@mail.com", false, Now);

        var again = workshop.Enroll(participant, [], Now);

        Assert.NotEqual(enrollment.Id, again.Id);
        Assert.True(again.IsActive);
        Assert.Equal(0, workshop.AvailableSeats);
    }
}
