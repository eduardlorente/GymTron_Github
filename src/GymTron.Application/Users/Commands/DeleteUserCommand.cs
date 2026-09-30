using FluentValidation;
using GymTron.Application.Base;

namespace GymTron.Application.Users.Commands;

public class DeleteUserCommand(Guid correlationId, int id, int? currentUserId) : CommandBase(correlationId)
{
    public int Id { get; } = id;
    public int? CurrentUserId { get; } = currentUserId;
}

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
