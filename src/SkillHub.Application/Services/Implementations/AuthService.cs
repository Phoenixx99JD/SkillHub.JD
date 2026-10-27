using SkillHub.Application.Abstractions.Persistence;
using SkillHub.Application.Abstractions.Services;
using SkillHub.Application.Common;
using SkillHub.Application.Common.Exceptions;
using SkillHub.Application.Common.Validation;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Application.Services.Interfaces;
using SkillHub.Domain.Entities;

namespace SkillHub.Application.Services.Implementations;

public class AuthService(
    IIdentityService identityService,
    ITokenService tokenService,
    IParticipantRepository participants,
    IInstructorRepository instructors,
    IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        RequestValidator.Validate(request);
        var email = request.Email.Trim().ToLowerInvariant();

        if (await identityService.EmailExistsAsync(email, ct))
            throw new ConflictException($"The email {email} is already registered.");

        Participant participant = null!;
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Self registration always creates a participant account.
            var userId = await identityService.CreateUserAsync(email, request.Password, Roles.Participant, ct);
            participant = new Participant(request.FullName, email, userId);
            await participants.AddAsync(participant, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }, ct);

        return new UserDto(participant.UserId, participant.FullName, participant.Email, [Roles.Participant]);
    }

    public async Task<UserDto> AuthenticateAsync(LoginRequest request, CancellationToken ct = default)
    {
        RequestValidator.Validate(request);

        var user = await identityService.ValidateCredentialsAsync(request.Email.Trim(), request.Password, ct)
                   ?? throw new UnauthorizedException("Invalid email or password.");

        var fullName = user.Roles.Contains(Roles.Employee)
            ? (await instructors.GetByUserIdAsync(user.UserId, ct))?.FullName
            : (await participants.GetByUserIdAsync(user.UserId, ct))?.FullName;

        return new UserDto(user.UserId, fullName ?? user.Email, user.Email, user.Roles);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await AuthenticateAsync(request, ct);
        var token = tokenService.CreateToken(user.UserId, user.Email, user.FullName, user.Roles);
        return new LoginResponse(token.Token, token.ExpiresAtUtc, user);
    }
}
