namespace SkillHub.Application.DTOs.Reports;

public record TopWorkshopDto(
    Guid Id,
    string Code,
    string Title,
    string InstructorName,
    DateTime StartsAt,
    int Capacity,
    int ActiveEnrollments);
