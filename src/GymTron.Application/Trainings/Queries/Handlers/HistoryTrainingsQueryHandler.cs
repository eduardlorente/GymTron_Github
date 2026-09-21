using GymTron.Application.Base;
using GymTron.Application.Trainings.Queries.DTO;
using GymTron.Domain.Aggregates;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Trainings.Queries.Handlers;

internal class HistoryTrainingsQueryHandler(ITrainingRepository trainingRepository, IExceptionLogger<HistoryTrainingsQuery> logger)
    : BaseQueryHandler<HistoryTrainingsQuery, List<TrainingHistoryDto>>(logger)
{
    private readonly ITrainingRepository _trainingRepository = trainingRepository;

    protected override async Task<List<TrainingHistoryDto>> HandleQuery(HistoryTrainingsQuery request, CancellationToken cancellationToken)
    {
        var projections = await _trainingRepository.ListCompletedHistory(request.UserId, cancellationToken);

        return projections.Select(t => new TrainingHistoryDto
        {
            StartedOn = t.StartedOn.FullDate,
            DayOfTheWeek = t.DayOfTheWeek
        }).ToList();
    }
}
