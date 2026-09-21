using FluentValidation;
using GymTron.Application.Auth.Models;
using GymTron.Application.Base;

namespace GymTron.Application.Auth.Commands.Login;

public class LoginCommand(Guid correlationId, string identifier, string password)
    : CommandBaseWithResponse<AuthResult>(correlationId)
{
    public string Identifier { get; } = identifier;
    public string Password { get; } = password;
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty()
            .WithMessage("Username or email is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}
