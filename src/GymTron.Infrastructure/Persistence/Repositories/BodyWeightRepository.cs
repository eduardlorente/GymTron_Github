using GymTron.Domain.Entities;
using GymTron.Domain.Projections;
using GymTron.Domain.Repositories;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;

namespace GymTron.Infrastructure.Persistence.Repositories;

internal class BodyWeightRepository(IBodyWeightDAL bodyWeightDAL) : IBodyWeightRepository
{
    private readonly IBodyWeightDAL _bodyWeightDAL = bodyWeightDAL;

    public async Task Add(BodyWeight entity, CancellationToken cancellationToken = default)
    {
        await _bodyWeightDAL.Add(entity, cancellationToken);
    }

    public async Task<List<BodyWeight>> ListAll(CancellationToken cancellationToken = default)
    {
        IEnumerable<BodyWeightDALModel> bodyWeights = await _bodyWeightDAL.ListAll(null, cancellationToken);

        return bodyWeights.Select(bw => BodyWeight.FromDatabase(bw.Id,
                                                                bw.Weight,
                                                                bw.BodyFatPercentage,
                                                                bw.CreatedOn,
                                                                bw.UserId))
                          .ToList();
    }

    public async Task<List<BodyWeightHistoryProjection>> ListHistory(int? userId = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<BodyWeightDALModel> bodyWeights = await _bodyWeightDAL.ListAll(userId, cancellationToken);

        return bodyWeights.Select(bw => new BodyWeightHistoryProjection
        {
            Weight = bw.Weight,
            BodyFatPercentage = bw.BodyFatPercentage,
            CreatedOn = bw.CreatedOn
        })
        .OrderByDescending(b => b.CreatedOn)
        .ToList();
    }
}
