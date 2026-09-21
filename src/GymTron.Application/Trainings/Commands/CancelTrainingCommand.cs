using FluentValidation;
using GymTron.Application.Base;
using GymTron.Domain.Aggregates;

namespace GymTron.Application.Trainings.Commands;

public class CancelTrainingCommand(Guid correlationId, Training training)
    : CommandBase(correlationId)
{


    public Training Training { get; private set; } = training;
}


public class CancelTrainingCommandValidator : AbstractValidator<CancelTrainingCommand>
{
    public CancelTrainingCommandValidator()
    {
        RuleFor(x => x.Training).NotNull();
    }
}
