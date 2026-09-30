using GymTron.Application.Base;
using GymTron.Application.Users.DTO;

namespace GymTron.Application.Users.Queries;

public class GetUserByIdQuery(Guid correlationId, int id) : QueryBase<UserDto?>(correlationId)
{
    public int Id { get; } = id;
}
