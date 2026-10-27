namespace SkillHub.Application.Common.Exceptions;

public abstract class AppException(string message) : Exception(message);

/// <summary>The requested resource does not exist (404).</summary>
public class NotFoundException(string message) : AppException(message);

/// <summary>The current user is not allowed to access the resource (403).</summary>
public class ForbiddenException(string message) : AppException(message);

/// <summary>Invalid credentials or missing authentication (401).</summary>
public class UnauthorizedException(string message) : AppException(message);

/// <summary>The request conflicts with the current state of a resource (409).</summary>
public class ConflictException(string message) : AppException(message);

/// <summary>The request payload is invalid (400).</summary>
public class ValidationException(IDictionary<string, string[]> errors)
    : AppException("One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;

    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] }) { }
}
