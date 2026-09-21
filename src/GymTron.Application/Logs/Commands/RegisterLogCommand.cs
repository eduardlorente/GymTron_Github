using FluentValidation;
using GymTron.Application.Base;

namespace GymTron.Application.Logs.Commands;

public class RegisterLogCommand(Guid correlationId, string message)
    : CommandBase(correlationId)
{


    public string Message { get; set; } = message;
}


public class RegisterLogCommandValidator : AbstractValidator<RegisterLogCommand>
{
    public RegisterLogCommandValidator()
    {
        RuleFor(x => x.Message).NotNull().NotEmpty();
    }
}
