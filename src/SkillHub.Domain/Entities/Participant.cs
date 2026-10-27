using SkillHub.Domain.Common;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Participant : Entity
{
    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;

    /// <summary>Identity account linked to this participant.</summary>
    public string UserId { get; private set; } = null!;

    private Participant() { }

    public Participant(string fullName, string email, string userId)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new DomainException("Participant full name is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Participant email is required.");

        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        UserId = userId;
    }
}
