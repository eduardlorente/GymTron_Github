using FluentValidation;
using GymTron.Application.Base;

namespace GymTron.Application.Routines.Commands;

public class UpdateRoutineCommand(Guid correlationId, int id, string name, List<RoutineItemInput> items, int? userId = null) 
    : CommandBase(correlationId)
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public List<RoutineItemInput> Items { get; } = items;
    public int? UserId { get; } = userId;
}


public class UpdateRoutineCommandValidator : AbstractValidator<UpdateRoutineCommand>
{
    public UpdateRoutineCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Items).NotNull().NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new RoutineItemInputValidator());
    }
}
