using GymTron.Domain.Entities;
using GymTron.Domain.Projections;
using GymTron.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace GymTron.Infrastructure.Persistence.Repositories;

internal class CachedRoutineRepository(IRoutineRepository innerRepository,
                                       IMemoryCache memoryCache) : IRoutineRepository
{
    private const string ALL_ROUTINES_CACHE_KEY = "AllRoutinesCacheKey";
    private const string ALL_ROUTINES_PROJECTIONS_CACHE_KEY = "AllRoutinesProjectionsCacheKey";

    private readonly IRoutineRepository _innerRepository = innerRepository;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task<List<Routine>> ListAll(int? userId = null, CancellationToken cancellationToken = default)
    {
        string cacheKey = userId.HasValue ? $"{ALL_ROUTINES_CACHE_KEY}_{userId.Value}" : ALL_ROUTINES_CACHE_KEY;
        if (!_memoryCache.TryGetValue(cacheKey, out List<Routine>? routines))
        {
            routines = await _innerRepository.ListAll(userId, cancellationToken);
            _memoryCache.Set(cacheKey, routines, TimeSpan.FromHours(3));
        }

        return routines ?? [];
    }

    public async Task<Routine?> GetById(int id, CancellationToken cancellationToken = default)
    {
        List<Routine> routines = await ListAll(null, cancellationToken);
        return routines.FirstOrDefault(r => r.Id == id);
    }

    public async Task<List<RoutineProjection>> ListRoutineProjections(int? userId = null, CancellationToken cancellationToken = default)
    {
        string cacheKey = userId.HasValue ? $"{ALL_ROUTINES_PROJECTIONS_CACHE_KEY}_{userId.Value}" : ALL_ROUTINES_PROJECTIONS_CACHE_KEY;
        if (!_memoryCache.TryGetValue(cacheKey, out List<RoutineProjection>? projections))
        {
            projections = await _innerRepository.ListRoutineProjections(userId, cancellationToken);
            _memoryCache.Set(cacheKey, projections, TimeSpan.FromHours(3));
        }

        return projections ?? [];
    }

    public async Task<RoutineProjection?> GetRoutineProjection(int id, CancellationToken cancellationToken = default)
    {
        List<RoutineProjection> projections = await ListRoutineProjections(null, cancellationToken);
        return projections.FirstOrDefault(r => r.Id == id);
    }

    public async Task<int> Create(Routine routine, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(ALL_ROUTINES_CACHE_KEY);
        _memoryCache.Remove(ALL_ROUTINES_PROJECTIONS_CACHE_KEY);
        if (routine.UserId.HasValue)
        {
            _memoryCache.Remove($"{ALL_ROUTINES_CACHE_KEY}_{routine.UserId.Value}");
            _memoryCache.Remove($"{ALL_ROUTINES_PROJECTIONS_CACHE_KEY}_{routine.UserId.Value}");
        }
        return await _innerRepository.Create(routine, cancellationToken);
    }

    public async Task Update(Routine routine, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(ALL_ROUTINES_CACHE_KEY);
        _memoryCache.Remove(ALL_ROUTINES_PROJECTIONS_CACHE_KEY);
        if (routine.UserId.HasValue)
        {
            _memoryCache.Remove($"{ALL_ROUTINES_CACHE_KEY}_{routine.UserId.Value}");
            _memoryCache.Remove($"{ALL_ROUTINES_PROJECTIONS_CACHE_KEY}_{routine.UserId.Value}");
        }
        await _innerRepository.Update(routine, cancellationToken);
    }
}
