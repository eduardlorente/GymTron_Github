using GymTron.Application.Base;
using GymTron.Application.Common.Events;
using GymTron.Domain.Common;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Trainings.Commands.Handlers;

internal class FinishTrainingCommandHandler(IDomainEventDispatcher eventDispatcher,
                                            ITrainingRepository trainingRepository,
                                            IExerciseRepository exerciseRepository,
                                            IUnitOfWork unitOfWork,
                                            IExceptionLogger<FinishTrainingCommand> logger,
                                            IClock clock)
    : BaseCommandHandler<FinishTrainingCommand>(logger)
{
    private readonly IDomainEventDispatcher _eventDispatcher = eventDispatcher;
    private readonly ITrainingRepository _trainingRepository = trainingRepository;
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IClock _clock = clock;

    protected override async Task HandleCommand(FinishTrainingCommand request, CancellationToken cancellationToken)
    {
        request.Training.Complete(_clock);

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _trainingRepository.Update(request.Training, ct);
            await _exerciseRepository.AddRange(request.Training.CompletedWorkout, ct);
        }, cancellationToken);

        await _eventDispatcher.DispatchAndClearEvents(request.Training, cancellationToken);
    }
}
