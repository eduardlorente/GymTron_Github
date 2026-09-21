using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Exceptions;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using ExerciseParametersEntity = GymTron.Domain.Entities.ExerciseParameters;

namespace GymTron.Application.Routines.Commands.Handlers;

internal class UpdateRoutineCommandHandler(IRoutineRepository routineRepository, IExceptionLogger<UpdateRoutineCommand> logger)
    : BaseCommandHandler<UpdateRoutineCommand>(logger)
{
    private readonly IRoutineRepository _routineRepository = routineRepository;

    protected override async Task HandleCommand(UpdateRoutineCommand request, CancellationToken cancellationToken)
    {
        int? routineUserId = null;
        if (request.UserId.HasValue)
        {
            Routine existing = await _routineRepository.GetById(request.Id, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(Routine));

            if (existing.UserId != request.UserId)
            {
                throw new EntityNotFoundException(nameof(Routine));
            }

            routineUserId = existing.UserId;
        }

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

        Routine routine = Routine.FromDatabase(request.Id, request.Name, items, routineUserId);

        await _routineRepository.Update(routine, cancellationToken);
    }
}
