using FluentValidation;
using GymTron.Application.Base;
using GymTron.Application.ExerciseParameters.Queries.DTO;

namespace GymTron.Application.ExerciseParameters.Queries;

public class GetExerciseParameterByIdQuery(Guid correlationId, int id) : QueryBase<ExerciseParameterDto?>(correlationId)
{
    public int Id { get; } = id;
}


public class GetExerciseParameterByIdQueryValidator : AbstractValidator<GetExerciseParameterByIdQuery>
{
    public GetExerciseParameterByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
