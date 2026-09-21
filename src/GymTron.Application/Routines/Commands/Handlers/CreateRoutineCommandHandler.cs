using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using ExerciseParametersEntity = GymTron.Domain.Entities.ExerciseParameters;

namespace GymTron.Application.Routines.Commands.Handlers;

internal class CreateRoutineCommandHandler(IRoutineRepository routineRepository, IExceptionLogger<CreateRoutineCommand> logger)
    : BaseCommandHandlerWithResponse<CreateRoutineCommand, int>(logger)
{
    private readonly IRoutineRepository _routineRepository = routineRepository;

    protected override async Task<int> HandleCommand(CreateRoutineCommand request, CancellationToken cancellationToken)
    {
        List<RoutineItem> items = request.Items.Select(i => RoutineItem.Create(
            i.DayOfWeek,
            ExerciseParametersEntity.FromDatabase(
                i.ExerciseParametersId,
                string.Empty,
                string.Empty,
                string.Empty,
                i.Series,
                (i.RepetitionsMin, i.RepetitionsMax),
                i.Duration ?? 0,
                null,
                (i.MinRestTimeInSeconds, i.MaxRestTimeInSeconds ?? 0),
                null,
                null,
                null,
                i.Type,
                []),
            i.AlternatingSeries,
            i.Position)).ToList();

        Routine routine = Routine.Create(request.Name, items, request.UserId);

        return await _routineRepository.Create(routine, cancellationToken);
    }
}
