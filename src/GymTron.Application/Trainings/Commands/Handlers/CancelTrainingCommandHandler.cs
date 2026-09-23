using GymTron.Application.Base;
using GymTron.Application.Common.Events;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Trainings.Commands.Handlers;

internal class CancelTrainingCommandHandler(IDomainEventDispatcher eventDispatcher,
                                            ITrainingRepository trainingRepository,
                                            IExceptionLogger<CancelTrainingCommand> logger,
                                            IClock clock)
    : BaseCommandHandler<CancelTrainingCommand>(logger)
{
    private readonly IDomainEventDispatcher _eventDispatcher = eventDispatcher;
    private readonly ITrainingRepository _trainingRepository = trainingRepository;
    private readonly IClock _clock = clock;

    protected override async Task HandleCommand(CancelTrainingCommand request, CancellationToken cancellationToken)
    {
        request.Training.Cancel(_clock);

        await _trainingRepository.Update(request.Training, cancellationToken);

        await _eventDispatcher.DispatchAndClearEvents(request.Training, cancellationToken);
    }
}
