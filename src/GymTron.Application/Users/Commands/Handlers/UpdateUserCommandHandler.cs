using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Exceptions;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Users.Commands.Handlers;

internal class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IClock clock,
    IExceptionLogger<UpdateUserCommand> logger)
    : BaseCommandHandler<UpdateUserCommand>(logger)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IClock _clock = clock;

    protected override async Task HandleCommand(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetById(request.Id, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException($"User with ID {request.Id} was not found.");
        }

        bool exists = await _userRepository.ExistsByUsernameOrEmail(request.Username, request.Email, request.Id, cancellationToken);
        if (exists)
        {
            throw new InvalidDomainOperationException("User with this username or email already exists.");
        }

        user.UpdateDetails(request.Username, request.Email, request.TypeId, _clock);

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            string passwordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.UpdatePassword(passwordHash, _clock);
        }

        if (request.IsActive != user.Status.IsActive)
        {
            if (request.IsActive)
            {
                user.Activate(_clock);
            }
            else
            {
                user.Deactivate(_clock);
            }
        }

        await _userRepository.Update(user, cancellationToken);
    }
}
