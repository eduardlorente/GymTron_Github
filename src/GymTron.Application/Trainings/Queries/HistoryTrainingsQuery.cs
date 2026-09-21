using GymTron.Application.Base;
using GymTron.Application.Trainings.Queries.DTO;

namespace GymTron.Application.Trainings.Queries;

public class HistoryTrainingsQuery(Guid correlationId, int? userId = null) : QueryBase<List<TrainingHistoryDto>>(correlationId)
{
    public int? UserId { get; } = userId;
}
