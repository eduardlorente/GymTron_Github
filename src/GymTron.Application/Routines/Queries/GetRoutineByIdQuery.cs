using FluentValidation;
using GymTron.Application.Base;
using GymTron.Application.Routines.Queries.DTO;

namespace GymTron.Application.Routines.Queries;

public class GetRoutineByIdQuery(Guid correlationId, int routineId, int? userId = null) : QueryBase<RoutineDto?>(correlationId)
{
    public int RoutineId { get; } = routineId;
    public int? UserId { get; } = userId;
}


public class GetRoutineByIdQueryValidator : AbstractValidator<GetRoutineByIdQuery>
{
    public GetRoutineByIdQueryValidator()
    {
        RuleFor(x => x.RoutineId).GreaterThan(0);
    }
}