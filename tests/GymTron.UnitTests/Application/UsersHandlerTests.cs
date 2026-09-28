using FluentValidation.TestHelper;
using GymTron.Application.Users.Commands;
using GymTron.Application.Users.Commands.Handlers;
using GymTron.Application.Users.Queries;
using GymTron.Application.Users.Queries.Handlers;
using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Exceptions;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using NSubstitute;

namespace GymTron.UnitTests.Application;

public class UsersHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IExceptionLogger<CreateUserCommand> _createLogger = Substitute.For<IExceptionLogger<CreateUserCommand>>();
    private readonly IExceptionLogger<UpdateUserCommand> _updateLogger = Substitute.For<IExceptionLogger<UpdateUserCommand>>();
    private readonly IExceptionLogger<DeleteUserCommand> _deleteLogger = Substitute.For<IExceptionLogger<DeleteUserCommand>>();
    private readonly IExceptionLogger<ListUsersQuery> _listLogger = Substitute.For<IExceptionLogger<ListUsersQuery>>();
    private readonly IExceptionLogger<GetUserByIdQuery> _getByIdLogger = Substitute.For<IExceptionLogger<GetUserByIdQuery>>();

    public UsersHandlerTests()
    {
        _clock.UtcNow.Returns(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashed_pwd");
    }

    [Fact]
    public async Task CreateUserCommandHandler_ValidCommand_HashesPasswordAndAddsUser()
    {
        var handler = new CreateUserCommandHandler(_userRepository, _passwordHasher, _clock, _createLogger);
        var command = new CreateUserCommand(Guid.NewGuid(), "newadmin", "admin@gymtron.local", "ValidPass123", UserTypes.Administrator);

        _userRepository.ExistsByUsernameOrEmail("newadmin", "admin@gymtron.local", Arg.Any<CancellationToken>())
            .Returns(false);
        _userRepository.Add(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(5);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(5, result);
        _passwordHasher.Received(1).HashPassword("ValidPass123");
        await _userRepository.Received(1).Add(Arg.Is<User>(u => u.Username == "newadmin" && u.TypeId == UserTypes.Administrator), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateUserCommandHandler_DuplicateUsernameOrEmail_ThrowsInvalidDomainOperationException()
    {
        var handler = new CreateUserCommandHandler(_userRepository, _passwordHasher, _clock, _createLogger);
        var command = new CreateUserCommand(Guid.NewGuid(), "existing", "exist@gymtron.local", "ValidPass123", UserTypes.Standard);

        _userRepository.ExistsByUsernameOrEmail("existing", "exist@gymtron.local", Arg.Any<CancellationToken>())
            .Returns(true);

        await Assert.ThrowsAsync<InvalidDomainOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void CreateUserCommandValidator_ValidationRules_WorkAsExpected()
    {
        var validator = new CreateUserCommandValidator();

        var invalidCommand = new CreateUserCommand(Guid.NewGuid(), "ab", "notanemail", "weak", UserTypes.Undefined);
        var result = validator.TestValidate(invalidCommand);

        result.ShouldHaveValidationErrorFor(x => x.Username);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.ShouldHaveValidationErrorFor(x => x.TypeId);

        var validCommand = new CreateUserCommand(Guid.NewGuid(), "validuser", "user@gymtron.local", "StrongPass1", UserTypes.Standard);
        var validResult = validator.TestValidate(validCommand);
        validResult.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task UpdateUserCommandHandler_ValidCommand_UpdatesDetailsAndSaves()
    {
        var handler = new UpdateUserCommandHandler(_userRepository, _passwordHasher, _clock, _updateLogger);
        var existingUser = User.FromDatabase(10, "olduser", "old@gymtron.local", "oldhash", UserTypes.Standard, true, DateTime.UtcNow);

        _userRepository.GetById(10, Arg.Any<CancellationToken>()).Returns(existingUser);
        _userRepository.ExistsByUsernameOrEmail("newuser", "new@gymtron.local", 10, Arg.Any<CancellationToken>()).Returns(false);

        var command = new UpdateUserCommand(Guid.NewGuid(), 10, "newuser", "new@gymtron.local", UserTypes.Administrator, false, "NewPassword123");
        await handler.Handle(command, CancellationToken.None);

        Assert.Equal("newuser", existingUser.Username);
        Assert.Equal("new@gymtron.local", existingUser.Email);
        Assert.Equal(UserTypes.Administrator, existingUser.TypeId);
        Assert.False(existingUser.Status.IsActive);
        _passwordHasher.Received(1).HashPassword("NewPassword123");
        await _userRepository.Received(1).Update(existingUser, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateUserCommandHandler_UserNotFound_ThrowsEntityNotFoundException()
    {
        var handler = new UpdateUserCommandHandler(_userRepository, _passwordHasher, _clock, _updateLogger);
        _userRepository.GetById(999, Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new UpdateUserCommand(Guid.NewGuid(), 999, "username", "email@gymtron.local", UserTypes.Standard, true);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteUserCommandHandler_SelfDeletion_ThrowsInvalidDomainOperationException()
    {
        var handler = new DeleteUserCommandHandler(_userRepository, _deleteLogger);
        var command = new DeleteUserCommand(Guid.NewGuid(), 2, 2);

        await Assert.ThrowsAsync<InvalidDomainOperationException>(() => handler.Handle(command, CancellationToken.None));
        await _userRepository.DidNotReceive().SoftDelete(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteUserCommandHandler_ValidTarget_CallsSoftDelete()
    {
        var handler = new DeleteUserCommandHandler(_userRepository, _deleteLogger);
        var existingUser = User.FromDatabase(5, "user5", "u5@gymtron.local", "hash", UserTypes.Standard, true, DateTime.UtcNow);
        _userRepository.GetById(5, Arg.Any<CancellationToken>()).Returns(existingUser);

        var command = new DeleteUserCommand(Guid.NewGuid(), 5, 2);
        await handler.Handle(command, CancellationToken.None);

        await _userRepository.Received(1).SoftDelete(5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListUsersQueryHandler_ReturnsProjectedUsers()
    {
        var handler = new ListUsersQueryHandler(_userRepository, _listLogger);
        var users = new List<User>
        {
            User.FromDatabase(1, "user1", "u1@gymtron.local", "hash1", UserTypes.Standard, true, new DateTime(2026, 1, 1)),
            User.FromDatabase(2, "admin", "admin@gymtron.local", "hash2", UserTypes.Administrator, true, new DateTime(2026, 1, 2))
        };
        _userRepository.GetAll(Arg.Any<CancellationToken>()).Returns(users);

        var result = await handler.Handle(new ListUsersQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(UserTypes.Standard, result[0].TypeId);
        Assert.Equal(UserTypes.Administrator, result[1].TypeId);
    }

    [Fact]
    public async Task GetUserByIdQueryHandler_UserFound_ReturnsDto()
    {
        var handler = new GetUserByIdQueryHandler(_userRepository, _getByIdLogger);
        var user = User.FromDatabase(3, "user3", "u3@gymtron.local", "hash3", UserTypes.Standard, true, new DateTime(2026, 1, 3));
        _userRepository.GetById(3, Arg.Any<CancellationToken>()).Returns(user);

        var result = await handler.Handle(new GetUserByIdQuery(Guid.NewGuid(), 3), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("user3", result.Username);
    }
}
