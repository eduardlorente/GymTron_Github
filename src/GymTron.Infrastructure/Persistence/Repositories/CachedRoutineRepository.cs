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

    public async Task<Routine?> GetById(int id, int? userId = null, CancellationToken cancellationToken = default)
    {
        string cacheKey = userId.HasValue ? $"Routine_{id}_{userId.Value}" : $"Routine_{id}";
        if (!_memoryCache.TryGetValue(cacheKey, out Routine? routine))
        {
            routine = await _innerRepository.GetById(id, userId, cancellationToken);
            if (routine is not null)
            {
                _memoryCache.Set(cacheKey, routine, TimeSpan.FromHours(3));
            }
        }

        return routine;
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

    public async Task<RoutineProjection?> GetRoutineProjection(int id, int? userId = null, CancellationToken cancellationToken = default)
    {
        string cacheKey = userId.HasValue ? $"RoutineProjection_{id}_{userId.Value}" : $"RoutineProjection_{id}";
        if (!_memoryCache.TryGetValue(cacheKey, out RoutineProjection? projection))
        {
            projection = await _innerRepository.GetRoutineProjection(id, userId, cancellationToken);
            if (projection is not null)
            {
                _memoryCache.Set(cacheKey, projection, TimeSpan.FromHours(3));
            }
        }

        return projection;
    }

    public async Task<int> Create(Routine routine, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(ALL_ROUTINES_CACHE_KEY);
        _memoryCache.Remove(ALL_ROUTINES_PROJECTIONS_CACHE_KEY);
        _memoryCache.Remove($"Routine_{routine.Id}");
        _memoryCache.Remove($"RoutineProjection_{routine.Id}");
        if (routine.UserId.HasValue)
        {
            _memoryCache.Remove($"{ALL_ROUTINES_CACHE_KEY}_{routine.UserId.Value}");
            _memoryCache.Remove($"{ALL_ROUTINES_PROJECTIONS_CACHE_KEY}_{routine.UserId.Value}");
            _memoryCache.Remove($"Routine_{routine.Id}_{routine.UserId.Value}");
            _memoryCache.Remove($"RoutineProjection_{routine.Id}_{routine.UserId.Value}");
        }
        int id = await _innerRepository.Create(routine, cancellationToken);
        if (id > 0 && id != routine.Id)
        {
            _memoryCache.Remove($"Routine_{id}");
            _memoryCache.Remove($"RoutineProjection_{id}");
            if (routine.UserId.HasValue)
            {
                _memoryCache.Remove($"Routine_{id}_{routine.UserId.Value}");
                _memoryCache.Remove($"RoutineProjection_{id}_{routine.UserId.Value}");
            }
        }
        return id;
    }

    public async Task Update(Routine routine, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(ALL_ROUTINES_CACHE_KEY);
        _memoryCache.Remove(ALL_ROUTINES_PROJECTIONS_CACHE_KEY);
        _memoryCache.Remove($"Routine_{routine.Id}");
        _memoryCache.Remove($"RoutineProjection_{routine.Id}");
        if (routine.UserId.HasValue)
        {
            _memoryCache.Remove($"{ALL_ROUTINES_CACHE_KEY}_{routine.UserId.Value}");
            _memoryCache.Remove($"{ALL_ROUTINES_PROJECTIONS_CACHE_KEY}_{routine.UserId.Value}");
            _memoryCache.Remove($"Routine_{routine.Id}_{routine.UserId.Value}");
            _memoryCache.Remove($"RoutineProjection_{routine.Id}_{routine.UserId.Value}");
        }
        await _innerRepository.Update(routine, cancellationToken);
    }
}
