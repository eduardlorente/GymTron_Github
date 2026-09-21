using GymTron.Application.Base;
using GymTron.Application.Common.Events;
using GymTron.Domain.Aggregates;
using GymTron.Domain.Entities;
using GymTron.Domain.Exceptions;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Trainings.Commands.Handlers;

internal class StartTrainingCommandHandler(IDomainEventDispatcher eventDispatcher,
                                           ITrainingRepository trainingRepository,
                                           IRoutineRepository routineRepository,
                                           IExceptionLogger<StartTrainingCommand> logger,
                                           IClock clock)
    : BaseCommandHandler<StartTrainingCommand>(logger)
{
    private readonly IDomainEventDispatcher _eventDispatcher = eventDispatcher;
    private readonly ITrainingRepository _trainingRepository = trainingRepository;
    private readonly IRoutineRepository _routineRepository = routineRepository;
    private readonly IClock _clock = clock;


    protected override async Task HandleCommand(StartTrainingCommand request, CancellationToken cancellationToken)
    {
        await AvoidStartTrainingIfExistsACurrentOne(request.UserId, cancellationToken);

        Routine routine = await _routineRepository.GetById(request.RoutineId, cancellationToken) ?? throw new EntityNotFoundException(nameof(Routine));

        if (request.UserId.HasValue && routine.UserId.HasValue && routine.UserId.Value != request.UserId.Value)
        {
            throw new EntityNotFoundException(nameof(Routine));
        }

        Training training = Training.CreateAnStartedTraining(request.RoutineId,
                                                             request.DayOfWeek,
                                                             routine.WorkByDays[request.DayOfWeek],
                                                             _clock,
                                                             request.UserId);

        await _trainingRepository.Add(training, cancellationToken);

        await _eventDispatcher.DispatchAndClearEvents(training, cancellationToken);
    }


    private async Task AvoidStartTrainingIfExistsACurrentOne(int? userId, CancellationToken cancellationToken)
    {
        Training? lastUserTraining = await _trainingRepository.GetCurrent(userId, cancellationToken);

        if (lastUserTraining != null)
            throw new InvalidDomainOperationException("The previous training is not ended.");
    }
}
