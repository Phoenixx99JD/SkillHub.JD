using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Tests.Domain;

public class WorkshopTests
{
    private static readonly DateTime Now = new(2026, 10, 1, 8, 0, 0);
    private static readonly Instructor Instructor = new("Ana Torres", "ana@skillhub.com", "Photography", "user-ana");

    private static Workshop NewWorkshop(string code = "FOT-001", int capacity = 2, DateTime? startsAt = null, int hours = 2)
    {
        var start = startsAt ?? Now.AddDays(5);
        return Workshop.Create(code, "Photography", "Basics", Instructor, start, start.AddHours(hours), capacity, Now);
    }

    private static Participant NewParticipant(string name = "p") => new(name, $"{name}@mail.com", $"user-{name}");

    [Fact]
    public void Create_InThePast_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            Workshop.Create("FOT-001", "T", "D", Instructor, Now.AddHours(-1), Now.AddHours(1), 10, Now));
        Assert.Contains("future", ex.Message);
    }

    [Fact]
    public void Create_EndBeforeStart_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Workshop.Create("FOT-001", "T", "D", Instructor, Now.AddDays(1), Now.AddDays(1).AddHours(-1), 10, Now));
    }

    [Fact]
    public void Create_NormalizesCode()
    {
        var workshop = Workshop.Create(" fot-001 ", "T", "D", Instructor, Now.AddDays(1), Now.AddDays(1).AddHours(1), 10, Now);
        Assert.Equal("FOT-001", workshop.Code);
    }

    [Fact]
    public void Enroll_ReducesAvailableSeats()
    {
        var workshop = NewWorkshop(capacity: 3);
        var enrollment = workshop.Enroll(NewParticipant(), [], Now);

        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);
        Assert.Equal(Now, enrollment.EnrolledAt);
        Assert.Equal(2, workshop.AvailableSeats);
    }

    [Fact]
    public void Enroll_WhenFull_Throws()
    {
        var workshop = NewWorkshop(capacity: 1);
        workshop.Enroll(NewParticipant("a"), [], Now);

        var ex = Assert.Throws<DomainException>(() => workshop.Enroll(NewParticipant("b"), [], Now));
        Assert.Contains("no available seats", ex.Message);
    }

    [Fact]
    public void Enroll_Twice_Throws()
    {
        var workshop = NewWorkshop();
        var participant = NewParticipant();
        workshop.Enroll(participant, [], Now);

        var ex = Assert.Throws<DomainException>(() => workshop.Enroll(participant, [], Now));
        Assert.Contains("already has an active enrollment", ex.Message);
    }

    [Fact]
    public void Enroll_InStartedWorkshop_Throws()
    {
        var workshop = NewWorkshop(startsAt: Now.AddHours(1));
        Assert.Throws<DomainException>(() => workshop.Enroll(NewParticipant(), [], Now.AddHours(1)));
    }

    [Theory]
    [InlineData(0, true)]   // same time
    [InlineData(1, true)]   // starts while the other is running
    [InlineData(2, false)]  // starts exactly when the other ends
    [InlineData(-2, false)] // ends exactly when the other starts
    public void Enroll_DetectsScheduleOverlaps(int offsetHours, bool overlaps)
    {
        var start = Now.AddDays(5);
        var other = NewWorkshop("COC-001", startsAt: start);
        var target = NewWorkshop("FOT-001", startsAt: start.AddHours(offsetHours));
        var participant = NewParticipant();

        if (overlaps)
            Assert.Throws<DomainException>(() => target.Enroll(participant, [other], Now));
        else
            Assert.NotNull(target.Enroll(participant, [other], Now));
    }

    [Fact]
    public void Enroll_ChangesConcurrencyVersion()
    {
        var workshop = NewWorkshop();
        var version = workshop.Version;
        workshop.Enroll(NewParticipant(), [], Now);
        Assert.NotEqual(version, workshop.Version);
    }
}
