using GymTron.Application.Base;
using GymTron.Application.Users.DTO;

namespace GymTron.Application.Users.Queries;

public class ListUsersQuery(Guid correlationId) : QueryBase<List<UserDto>>(correlationId)
{
}
