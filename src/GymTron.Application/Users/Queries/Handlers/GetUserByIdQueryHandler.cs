using GymTron.Application.Base;
using GymTron.Application.Users.DTO;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Users.Queries.Handlers;

internal class GetUserByIdQueryHandler(
    IUserRepository userRepository,
    IExceptionLogger<GetUserByIdQuery> logger)
    : BaseQueryHandler<GetUserByIdQuery, UserDto?>(logger)
{
    private readonly IUserRepository _userRepository = userRepository;

    protected override async Task<UserDto?> HandleQuery(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetById(request.Id, cancellationToken);
        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            TypeId = user.TypeId,
            IsActive = user.Status.IsActive,
            CreatedAt = user.Status.CreatedOn
        };
    }
}
