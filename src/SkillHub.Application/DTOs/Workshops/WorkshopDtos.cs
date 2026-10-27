using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs.Workshops;

public class CreateWorkshopRequest
{
    /// <summary>Unique code, e.g. FOT-001.</summary>
    [Required, StringLength(20), RegularExpression(@"^[A-Za-z]{2,10}-\d{1,6}$",
        ErrorMessage = "The code must have the format ABC-001.")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string InstructorEmail { get; set; } = string.Empty;

    [Required]
    public DateTime StartsAt { get; set; }

    [Required]
    public DateTime EndsAt { get; set; }

    [Range(1, 1000)]
    public int Capacity { get; set; }
}

public record WorkshopDto(
    Guid Id,
    string Code,
    string Title,
    string Description,
    string InstructorName,
    string InstructorEmail,
    string InstructorSpecialty,
    DateTime StartsAt,
    DateTime EndsAt,
    int Capacity,
    int AvailableSeats);
