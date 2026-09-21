using GymTron.Application.Base;
using GymTron.Application.ExerciseParameters.Queries.DTO;

namespace GymTron.Application.ExerciseParameters.Queries;

public class ListExerciseParametersQuery(Guid correlationId) : QueryBase<List<ExerciseParameterDto>>(correlationId)
{
}
