using FluentValidation;
using GymTron.Application.Base;

namespace GymTron.Application.Trainings.Commands;

public class StartTrainingCommand(Guid correlationId, int routineId, int dayOfWeek, int? userId = null)
    : CommandBase(correlationId)
{


    public int RoutineId { get; private set; } = routineId;
    public int DayOfWeek { get; private set; } = dayOfWeek;
    public int? UserId { get; private set; } = userId;
}


public class StartTrainingCommandValidator : AbstractValidator<StartTrainingCommand>
{
    public StartTrainingCommandValidator()
    {
        RuleFor(x => x.RoutineId).NotEmpty().GreaterThan(0);
        RuleFor(x => x.DayOfWeek).InclusiveBetween(1, 7);
    }
}
