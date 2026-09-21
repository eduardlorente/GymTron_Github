using FluentValidation;
using GymTron.Application.Base;
using GymTron.Domain.Aggregates;

namespace GymTron.Application.Trainings.Commands;

public class FinishTrainingCommand(Guid correlationId, Training training)
    : CommandBase(correlationId)
{


    public Training Training { get; private set; } = training;
}


public class FinishTrainingCommandValidator : AbstractValidator<FinishTrainingCommand>
{
    public FinishTrainingCommandValidator()
    {
        RuleFor(x => x.Training).NotNull();
    }
}
