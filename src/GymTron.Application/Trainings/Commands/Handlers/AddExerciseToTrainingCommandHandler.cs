using GymTron.Application.Base;
using GymTron.Domain.Aggregates;
using GymTron.Domain.Entities;
using GymTron.Domain.Services;

namespace GymTron.Application.Trainings.Commands.Handlers;

internal class AddExerciseToTrainingCommandHandler(IExceptionLogger<AddExerciseToTrainingCommand> logger,
                                                   IClock clock)
    : BaseCommandHandlerWithResponse<AddExerciseToTrainingCommand, Training>(logger)
{


    private readonly IClock _clock = clock;


    protected override async Task<Training> HandleCommand(AddExerciseToTrainingCommand request, CancellationToken cancellationToken)
    {
        Exercise exercise = Exercise.New(request.CurrentTraining!.Id!,
                                         request.ExerciseParametersId,
                                         request.ExerciseParametersName,
                                         request.Weight ?? 0,
                                         request.DurationInSeconds ?? 0,
                                         request.Repetitions ?? 0,
                                         request.Observations ?? [],
                                         _clock);

        request.CurrentTraining.CompleteExercise(exercise, _clock);

        return request.CurrentTraining;
    }
}
