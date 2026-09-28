using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Exceptions;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Users.Commands.Handlers;

internal class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IClock clock,
    IExceptionLogger<CreateUserCommand> logger)
    : BaseCommandHandlerWithResponse<CreateUserCommand, int>(logger)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IClock _clock = clock;

    protected override async Task<int> HandleCommand(CreateUserCommand request, CancellationToken cancellationToken)
    {
        bool exists = await _userRepository.ExistsByUsernameOrEmail(request.Username, request.Email, cancellationToken);
        if (exists)
        {
            throw new InvalidDomainOperationException("User with this username or email already exists.");
        }

        string passwordHash = _passwordHasher.HashPassword(request.Password);
        User user = User.New(request.Username, request.Email, passwordHash, request.TypeId, _clock);

        return await _userRepository.Add(user, cancellationToken);
    }
}
