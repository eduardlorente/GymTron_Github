using GymTron.Application.Base;
using GymTron.Application.Trainings.Queries.DTO;

namespace GymTron.Application.Trainings.Queries;

public class CurrentTrainingQuery(Guid correlationId, int? userId = null) : QueryBase<TrainingDto?>(correlationId)
{
    public int? UserId { get; } = userId;
}
