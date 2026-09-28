using GymTron.Application.Base;
using GymTron.Application.Users.DTO;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Users.Queries.Handlers;

internal class ListUsersQueryHandler(
    IUserRepository userRepository,
    IExceptionLogger<ListUsersQuery> logger)
    : BaseQueryHandler<ListUsersQuery, List<UserDto>>(logger)
{
    private readonly IUserRepository _userRepository = userRepository;

    protected override async Task<List<UserDto>> HandleQuery(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAll(cancellationToken);

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            TypeId = u.TypeId,
            IsActive = u.Status.IsActive,
            CreatedAt = u.Status.CreatedOn
        }).ToList();
    }
}
