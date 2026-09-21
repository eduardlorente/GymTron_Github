using GymTron.Application.Base;
using GymTron.Application.Common.Events;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Trainings.Commands.Handlers;

internal class FinishTrainingCommandHandler(IDomainEventDispatcher eventDispatcher,
                                            ITrainingRepository trainingRepository,
                                            IExerciseRepository exerciseRepository,
                                            IExceptionLogger<FinishTrainingCommand> logger,
                                            IClock clock)
    : BaseCommandHandler<FinishTrainingCommand>(logger)
{
    private readonly IDomainEventDispatcher _eventDispatcher = eventDispatcher;
    private readonly ITrainingRepository _trainingRepository = trainingRepository;
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository;
    private readonly IClock _clock = clock;

    protected override async Task HandleCommand(FinishTrainingCommand request, CancellationToken cancellationToken)
    {
        request.Training.Complete(_clock);

        await _trainingRepository.Update(request.Training, cancellationToken);

        await _exerciseRepository.AddRange(request.Training.CompletedWorkout, cancellationToken);

        await _eventDispatcher.DispatchAndClearEvents(request.Training, cancellationToken);
    }
}
