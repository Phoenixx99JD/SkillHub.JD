namespace SkillHub.Domain.Exceptions;

/// <summary>
/// Raised when an operation would break a business rule.
/// </summary>
public class DomainException(string message) : Exception(message);
