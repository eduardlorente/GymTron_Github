using GymTron.Application.Base;
using GymTron.Domain.Projections;

namespace GymTron.Application.BodyWeights.Queries;

public class HistoryBodyWeightQuery(Guid correlationId, int? userId = null) : QueryBase<List<BodyWeightHistoryProjection>>(correlationId)
{
    public int? UserId { get; } = userId;
}
