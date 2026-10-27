using SkillHub.Domain.Common;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Instructor : Entity
{
    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Specialty { get; private set; } = null!;

    /// <summary>Identity account linked to this instructor.</summary>
    public string UserId { get; private set; } = null!;

    private Instructor() { }

    public Instructor(string fullName, string email, string specialty, string userId)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new DomainException("Instructor full name is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Instructor email is required.");
        if (string.IsNullOrWhiteSpace(specialty)) throw new DomainException("Instructor specialty is required.");

        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Specialty = specialty.Trim();
        UserId = userId;
    }
}
