namespace SkillHub.Infrastructure.Seeding;

public class SeedOptions
{
    public const string SectionName = "Seed";

    /// <summary>Path to SkillHub_Data.xlsx (absolute or relative to the content root).</summary>
    public string ExcelPath { get; set; } = "../../data/SkillHub_Data.xlsx";

    /// <summary>Password for seeded employee (instructor) accounts whose row has no Password column.</summary>
    public string EmployeePassword { get; set; } = "Instructor2026*";

    /// <summary>Password for seeded participant accounts whose row has no Password column.</summary>
    public string ParticipantPassword { get; set; } = "Skillhub2026*";
}
