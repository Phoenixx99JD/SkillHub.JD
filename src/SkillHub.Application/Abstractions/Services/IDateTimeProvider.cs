namespace SkillHub.Application.Abstractions.Services;

/// <summary>
/// Provides the current date and time in SkillHub's local time zone. Workshops are in-person, so
/// every date in the system is expressed in the center's local time.
/// </summary>
public interface IDateTimeProvider
{
    DateTime Now { get; }
}
