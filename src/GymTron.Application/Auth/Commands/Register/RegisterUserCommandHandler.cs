using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Exceptions;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Auth.Commands.Register;

internal class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IClock clock,
    IExceptionLogger<RegisterUserCommand> logger)
    : BaseCommandHandlerWithResponse<RegisterUserCommand, int>(logger)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IClock _clock = clock;

    protected override async Task<int> HandleCommand(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        bool exists = await _userRepository.ExistsByUsernameOrEmail(request.Username, request.Email, cancellationToken);
        if (exists)
        {
            throw new InvalidDomainOperationException("User with this username or email already exists.");
        }

        string passwordHash = _passwordHasher.HashPassword(request.Password);
        User user = User.New(request.Username, request.Email, passwordHash, _clock);

        return await _userRepository.Add(user, cancellationToken);
    }
}
