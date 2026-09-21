using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using ExerciseParametersEntity = GymTron.Domain.Entities.ExerciseParameters;

namespace GymTron.Application.ExerciseParameters.Commands.Handlers;

internal class UpdateExerciseParameterCommandHandler(IExerciseParameterRepository repository,
    IExceptionLogger<UpdateExerciseParameterCommand> logger)
    : BaseCommandHandler<UpdateExerciseParameterCommand>(logger)
{
    private readonly IExerciseParameterRepository _repository = repository;

    protected override async Task HandleCommand(UpdateExerciseParameterCommand request, CancellationToken cancellationToken)
    {
        ExerciseParametersEntity parameters = ExerciseParametersEntity.FromDatabase(
            request.Id,
            request.Name,
            request.Description,
            request.Pattern,
            0,
            (0, 0),
            0,
            request.ReplaysInReserve,
            (0, 0),
            null,
            null,
            null,
            request.Type,
            []);

        await _repository.Update(parameters, cancellationToken);
    }
}
