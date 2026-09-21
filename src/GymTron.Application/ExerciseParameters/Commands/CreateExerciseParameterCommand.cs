using FluentValidation;
using GymTron.Application.Base;
using GymTron.Domain.Enums;

namespace GymTron.Application.ExerciseParameters.Commands;

public class CreateExerciseParameterCommand(Guid correlationId, string name, string description,
    string pattern, ExerciseTypes type, int? replaysInReserve)
    : CommandBaseWithResponse<int>(correlationId)
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public string Pattern { get; } = pattern;
    public ExerciseTypes Type { get; } = type;
    public int? ReplaysInReserve { get; } = replaysInReserve;
}


public class CreateExerciseParameterCommandValidator : AbstractValidator<CreateExerciseParameterCommand>
{
    public CreateExerciseParameterCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.ReplaysInReserve).GreaterThanOrEqualTo(0).When(x => x.ReplaysInReserve.HasValue);
    }
}