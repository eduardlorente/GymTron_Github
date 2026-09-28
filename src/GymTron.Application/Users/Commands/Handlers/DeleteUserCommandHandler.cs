using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Exceptions;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Users.Commands.Handlers;

internal class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IExceptionLogger<DeleteUserCommand> logger)
    : BaseCommandHandler<DeleteUserCommand>(logger)
{
    private readonly IUserRepository _userRepository = userRepository;

    protected override async Task HandleCommand(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.CurrentUserId.HasValue && request.Id == request.CurrentUserId.Value)
        {
            throw new InvalidDomainOperationException("You cannot delete your own account.");
        }

        User? user = await _userRepository.GetById(request.Id, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException($"User with ID {request.Id} was not found.");
        }

        await _userRepository.SoftDelete(request.Id, cancellationToken);
    }
}
