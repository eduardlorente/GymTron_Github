using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using ExerciseParametersEntity = GymTron.Domain.Entities.ExerciseParameters;

namespace GymTron.Application.ExerciseParameters.Commands.Handlers;

internal class CreateExerciseParameterCommandHandler(IExerciseParameterRepository repository, 
    IExceptionLogger<CreateExerciseParameterCommand> logger)
    : BaseCommandHandlerWithResponse<CreateExerciseParameterCommand, int>(logger)
{
    private readonly IExerciseParameterRepository _repository = repository;

    protected override async Task<int> HandleCommand(CreateExerciseParameterCommand request, CancellationToken cancellationToken)
    {
        ExerciseParametersEntity parameters = ExerciseParametersEntity.Create(
            request.Name,
            request.Description,
            request.Pattern,
            request.Type,
            request.ReplaysInReserve);

        return await _repository.Create(parameters, cancellationToken);
    }
}
