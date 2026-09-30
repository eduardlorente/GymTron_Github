using FluentValidation;
using GymTron.Application.Base;
using GymTron.Domain.Enums;

namespace GymTron.Application.Users.Commands;

public class UpdateUserCommand(
    Guid correlationId,
    int id,
    string username,
    string email,
    UserTypes typeId,
    bool isActive,
    string? newPassword = null)
    : CommandBase(correlationId)
{
    public int Id { get; } = id;
    public string Username { get; } = username;
    public string Email { get; } = email;
    public UserTypes TypeId { get; } = typeId;
    public bool IsActive { get; } = isActive;
    public string? NewPassword { get; } = newPassword;
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.TypeId)
            .IsInEnum()
            .NotEqual(UserTypes.Undefined).WithMessage("User type must be specified.");

        RuleFor(x => x.NewPassword)
            .MinimumLength(6)
            .MaximumLength(128)
            .When(x => !string.IsNullOrEmpty(x.NewPassword));
    }
}
