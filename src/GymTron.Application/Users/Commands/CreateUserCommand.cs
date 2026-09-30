using FluentValidation;
using GymTron.Application.Base;
using GymTron.Domain.Enums;

namespace GymTron.Application.Users.Commands;

public class CreateUserCommand(Guid correlationId, string username, string email, string password, UserTypes typeId)
    : CommandBaseWithResponse<int>(correlationId)
{
    public string Username { get; } = username;
    public string Email { get; } = email;
    public string Password { get; } = password;
    public UserTypes TypeId { get; } = typeId;
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
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

        RuleFor(x => x.TypeId)
            .IsInEnum()
            .NotEqual(UserTypes.Undefined).WithMessage("User type must be specified.");
    }
}
