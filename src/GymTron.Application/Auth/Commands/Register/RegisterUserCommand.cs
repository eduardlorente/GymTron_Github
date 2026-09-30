using FluentValidation;
using GymTron.Application.Base;

namespace GymTron.Application.Auth.Commands.Register;

public class RegisterUserCommand(Guid correlationId, string username, string email, string password)
    : CommandBaseWithResponse<int>(correlationId)
{
    public string Username { get; } = username;
    public string Email { get; } = email;
    public string Password { get; } = password;
}

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(128);
    }
}
