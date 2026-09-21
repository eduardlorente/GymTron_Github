using FluentValidation;
using GymTron.Application.Auth.Models;
using GymTron.Application.Base;

namespace GymTron.Application.Auth.Commands.Refresh;

public class RefreshTokenCommand(Guid correlationId, string refreshToken)
    : CommandBaseWithResponse<AuthResult>(correlationId)
{
    public string RefreshToken { get; } = refreshToken;
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}
