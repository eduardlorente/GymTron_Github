using FluentValidation;
using GymTron.Application.Base;
using GymTron.Domain.Aggregates;

namespace GymTron.Application.Trainings.Commands;

public class AddExerciseToTrainingCommand : CommandBaseWithResponse<Training>
{


    public Training? CurrentTraining { get; private set; }
    public int ExerciseParametersId { get; private set; }
    public string ExerciseParametersName { get; private set; }
    public decimal? Weight { get; private set; }
    public int? Repetitions { get; private set; }
    public int? DurationInSeconds { get; private set; }
    public List<string> Observations { get; internal set; }


    private AddExerciseToTrainingCommand(Guid correlationId,
                                         Training currentTraining,
                                         int exerciseParametersId,
                                         string exerciseParametersName,
                                         decimal? weight,
                                         int? repetitions,
                                         int? duration,
                                         List<string> observations)
        : base(correlationId)
    {
        CurrentTraining = currentTraining;
        ExerciseParametersId = exerciseParametersId;
        ExerciseParametersName = exerciseParametersName;
        Weight = weight;
        Repetitions = repetitions;
        DurationInSeconds = duration;
        Observations = observations;
    }


    public static AddExerciseToTrainingCommand New(Guid correlationId,
                                                   Training currentTraining,
                                                   int exerciseParametersId,
                                                   string exerciseParametersName,
                                                   decimal? weight,
                                                   int? repetitions,
                                                   List<string> observations)
    {
        return new(correlationId, currentTraining, exerciseParametersId, exerciseParametersName, weight, repetitions, null, observations);
    }


    public static AddExerciseToTrainingCommand New(Guid correlationId,
                                                   Training currentTraining,
                                                   int exerciseParametersId,
                                                   string exerciseParametersName,
                                                   int? durationInSeconds,
                                                   List<string> observations)
    {
        return new(correlationId, currentTraining, exerciseParametersId, exerciseParametersName, null, null, durationInSeconds, observations);
    }
}


public class AddExerciseToTrainingCommandValidator : AbstractValidator<AddExerciseToTrainingCommand>
{
    public AddExerciseToTrainingCommandValidator()
    {
        RuleFor(x => x.CurrentTraining).NotNull();
        RuleFor(x => x.CurrentTraining!.Id).NotEmpty();
        RuleFor(x => x.ExerciseParametersId).GreaterThan(0);
        RuleFor(x => x.ExerciseParametersName).NotEmpty();

        When(x => x.DurationInSeconds.HasValue, () =>
        {
            RuleFor(x => x.DurationInSeconds!.Value)
                .GreaterThan(0)
                .WithMessage("Duration must be greater than zero.");
        }).Otherwise(() =>
        {
            RuleFor(x => x.Weight)
                .NotNull()
                .WithMessage("Weight is required.")
                .GreaterThan(0)
                .WithMessage("Weight must be greater than zero.");

            RuleFor(x => x.Repetitions)
                .NotNull()
                .WithMessage("Repetitions are required.")
                .GreaterThan(0)
                .WithMessage("Repetitions must be greater than zero.");
        });
    }
}
