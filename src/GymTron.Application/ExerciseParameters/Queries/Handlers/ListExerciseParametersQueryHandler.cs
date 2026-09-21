using GymTron.Application.Base;
using GymTron.Application.ExerciseParameters.Queries.DTO;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.ExerciseParameters.Queries.Handlers;

internal class ListExerciseParametersQueryHandler(IExerciseParameterRepository repository,
    IExceptionLogger<ListExerciseParametersQuery> logger)
    : BaseQueryHandler<ListExerciseParametersQuery, List<ExerciseParameterDto>>(logger)
{
    private readonly IExerciseParameterRepository _repository = repository;

    protected override async Task<List<ExerciseParameterDto>> HandleQuery(ListExerciseParametersQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.ListProjections(cancellationToken);

        return items.Select(e => new ExerciseParameterDto
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            Pattern = e.Pattern,
            Type = e.Type,
            ReplaysInReserve = e.ReplaysInReserve
        }).ToList();
    }
}
