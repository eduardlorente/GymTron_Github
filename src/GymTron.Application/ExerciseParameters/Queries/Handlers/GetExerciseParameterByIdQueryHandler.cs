using GymTron.Application.Base;
using GymTron.Application.ExerciseParameters.Queries.DTO;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.ExerciseParameters.Queries.Handlers;

internal class GetExerciseParameterByIdQueryHandler(IExerciseParameterRepository repository,
    IExceptionLogger<GetExerciseParameterByIdQuery> logger)
    : BaseQueryHandler<GetExerciseParameterByIdQuery, ExerciseParameterDto?>(logger)
{
    private readonly IExerciseParameterRepository _repository = repository;

    protected override async Task<ExerciseParameterDto?> HandleQuery(GetExerciseParameterByIdQuery request, CancellationToken cancellationToken)
    {
        var projection = await _repository.GetProjection(request.Id, cancellationToken);

        if (projection == null)
            return null;

        return new ExerciseParameterDto
        {
            Id = projection.Id,
            Name = projection.Name,
            Description = projection.Description,
            Pattern = projection.Pattern,
            Type = projection.Type,
            ReplaysInReserve = projection.ReplaysInReserve
        };
    }
}
