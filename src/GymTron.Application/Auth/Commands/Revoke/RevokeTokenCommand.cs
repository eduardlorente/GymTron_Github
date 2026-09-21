using FluentValidation;
using GymTron.Application.Base;

namespace GymTron.Application.Auth.Commands.Revoke;

public class RevokeTokenCommand(Guid correlationId, string refreshToken)
    : CommandBase(correlationId)
{
    public string RefreshToken { get; } = refreshToken;
}

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}
