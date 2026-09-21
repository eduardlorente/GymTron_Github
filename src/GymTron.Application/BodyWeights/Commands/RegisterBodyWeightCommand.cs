using FluentValidation;
using GymTron.Application.Base;

namespace GymTron.Application.BodyWeights.Commands;

public class RegisterBodyWeightCommand(Guid correlationId, decimal weight, decimal bodyFatPercentage = 0, int? userId = null)
    : CommandBase(correlationId)
{


    public decimal Weight { get; private set; } = weight;
    public decimal BodyFatPercentage { get; private set; } = bodyFatPercentage;
    public int? UserId { get; private set; } = userId;
}


public class RegisterBodyWeightCommandValidator : AbstractValidator<RegisterBodyWeightCommand>
{
    public RegisterBodyWeightCommandValidator()
    {
        RuleFor(x => x.Weight).NotEmpty().GreaterThan(0);
    }
}
