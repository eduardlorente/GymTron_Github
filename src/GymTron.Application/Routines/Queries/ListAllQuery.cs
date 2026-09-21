using GymTron.Application.Base;
using GymTron.Application.Routines.Queries.DTO;

namespace GymTron.Application.Routines.Queries;

public class ListAllQuery(Guid correlationId, int? userId = null) : QueryBase<List<RoutineDto>>(correlationId)
{
    public int? UserId { get; } = userId;
}
