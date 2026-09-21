using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Projections;
using GymTron.Domain.Repositories;
using GymTron.Infrastructure.Persistence.DAL.MySQL;

namespace GymTron.Infrastructure.Persistence.Repositories;

internal class ExerciseParameterRepository(IExerciseParameterDAL dal) : IExerciseParameterRepository
{
    private readonly IExerciseParameterDAL _dal = dal;

    public async Task<List<ExerciseParameters>> ListAll(CancellationToken cancellationToken = default)
    {
        var items = await _dal.ListAll(cancellationToken);

        return items.Select(e => ExerciseParameters.FromDatabase(
            e.Id,
            e.Name,
            e.Description,
            e.Pattern,
            0,
            (0, 0),
            0,
            e.ReplaysInReserve,
            (0, 0),
            null,
            null,
            null,
            (ExerciseTypes)e.TypeId,
            []
        )).ToList();
    }

    public async Task<ExerciseParameters?> GetById(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dal.GetById(id, cancellationToken);

        if (entity == null)
            return null;

        return ExerciseParameters.FromDatabase(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Pattern,
            0,
            (0, 0),
            0,
            entity.ReplaysInReserve,
            (0, 0),
            null,
            null,
            null,
            (ExerciseTypes)entity.TypeId,
            []
        );
    }

    public async Task<ExerciseParameterProjection?> GetProjection(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dal.GetById(id, cancellationToken);
        if (entity == null)
            return null;

        return new ExerciseParameterProjection
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Pattern = entity.Pattern,
            Type = (ExerciseTypes)entity.TypeId,
            ReplaysInReserve = entity.ReplaysInReserve
        };
    }

    public async Task<List<ExerciseParameterProjection>> ListProjections(CancellationToken cancellationToken = default)
    {
        var items = await _dal.ListAll(cancellationToken);

        return items.Select(e => new ExerciseParameterProjection
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            Pattern = e.Pattern,
            Type = (ExerciseTypes)e.TypeId,
            ReplaysInReserve = e.ReplaysInReserve
        }).ToList();
    }

    public async Task<int> Create(ExerciseParameters exerciseParameters, CancellationToken cancellationToken = default)
    {
        return await _dal.Create(
            exerciseParameters.Name,
            exerciseParameters.Description,
            exerciseParameters.Pattern,
            (int)exerciseParameters.Type,
            exerciseParameters.ReplaysInReserve,
            cancellationToken);
    }

    public async Task Update(ExerciseParameters exerciseParameters, CancellationToken cancellationToken = default)
    {
        await _dal.Update(
            exerciseParameters.Id,
            exerciseParameters.Name,
            exerciseParameters.Description,
            exerciseParameters.Pattern,
            (int)exerciseParameters.Type,
            exerciseParameters.ReplaysInReserve,
            cancellationToken);
    }
}
