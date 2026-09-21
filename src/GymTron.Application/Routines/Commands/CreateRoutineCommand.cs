using FluentValidation;
using GymTron.Application.Base;
using GymTron.Domain.Enums;

namespace GymTron.Application.Routines.Commands;

public class CreateRoutineCommand(Guid correlationId, string name, List<RoutineItemInput> items, int? userId = null)
    : CommandBaseWithResponse<int>(correlationId)
{
    public string Name { get; } = name;
    public List<RoutineItemInput> Items { get; } = items;
    public int? UserId { get; } = userId;
}


public class CreateRoutineCommandValidator : AbstractValidator<CreateRoutineCommand>
{
    public CreateRoutineCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Items).NotNull().NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new RoutineItemInputValidator());
    }
}


public class RoutineItemInputValidator : AbstractValidator<RoutineItemInput>
{
    public RoutineItemInputValidator()
    {
        RuleFor(x => x.DayOfWeek).InclusiveBetween(1, 7);
        RuleFor(x => x.ExerciseParametersId).GreaterThan(0);
        RuleFor(x => x.Series).GreaterThan(0);
        RuleFor(x => x.RepetitionsMin).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RepetitionsMax).GreaterThanOrEqualTo(x => x.RepetitionsMin)
            .When(x => x.RepetitionsMin >= 0);
        RuleFor(x => x.Duration).GreaterThan(0).When(x => x.Duration.HasValue);
        RuleFor(x => x.MinRestTimeInSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxRestTimeInSeconds).GreaterThanOrEqualTo(x => x.MinRestTimeInSeconds)
            .When(x => x.MaxRestTimeInSeconds.HasValue);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Type).IsInEnum();
    }
}


public class RoutineItemInput
{
    public int DayOfWeek { get; set; }
    public int ExerciseParametersId { get; set; }
    public int Series { get; set; }
    public int RepetitionsMin { get; set; }
    public int RepetitionsMax { get; set; }
    public int? Duration { get; set; }
    public int MinRestTimeInSeconds { get; set; }
    public int? MaxRestTimeInSeconds { get; set; }
    public bool AlternatingSeries { get; set; }
    public int Position { get; set; }
    public ExerciseTypes Type { get; set; }
}
