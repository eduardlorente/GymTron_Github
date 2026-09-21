using GymTron.Application.Base;
using GymTron.Domain.Projections;

namespace GymTron.Application.Exercises.Queries;

public class ExerciseHistoryQuery(Guid correlationId, int? userId = null) : QueryBase<List<ExerciseHistoryProjection>>(correlationId)
{
    public int? UserId { get; } = userId;
}
