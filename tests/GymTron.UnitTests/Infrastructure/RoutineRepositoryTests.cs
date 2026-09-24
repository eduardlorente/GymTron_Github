using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Projections;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace GymTron.UnitTests.Infrastructure;

public class RoutineRepositoryTests
{
    [Fact]
    public async Task ListAndGet_MapGroupedRoutinesAndReuseCachedData()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.ListAll().Returns([
            CreateRow(2, "Second", 21, 2, string.Empty, null, null),
            CreateRow(1, "First", 11, 1, "good;stable", 50, 8),
            CreateRow(1, "First", 12, 3, string.Empty, null, null)
        ]);
        dal.ListById(1).Returns([
            CreateRow(1, "First", 11, 1, "good;stable", 50, 8),
            CreateRow(1, "First", 12, 3, string.Empty, null, null)
        ]);
        dal.ListById(99).Returns([]);
        using MemoryCache cache = new(new MemoryCacheOptions());
        CachedRoutineRepository repository = new(new RoutineRepository(dal), cache);

        List<GymTron.Domain.Entities.Routine> routines = await repository.ListAll();
        GymTron.Domain.Entities.Routine? found = await repository.GetById(1);
        GymTron.Domain.Entities.Routine? cachedFound = await repository.GetById(1);
        GymTron.Domain.Entities.Routine? missing = await repository.GetById(99);

        await dal.Received(1).ListAll();
        await dal.Received(1).ListById(1, cancellationToken: Arg.Any<CancellationToken>());
        Assert.Equal([2, 1], routines.Select(item => item.Id));
        List<GymTron.Domain.Entities.RoutineItem> items = found!.WorkByDays.Values.SelectMany(value => value).ToList();
        Assert.Equal(2, items.Count);
        Assert.Equal([11, 12], items.Select(item => item.ExerciseParameters.Id));
        Assert.Equal(["good", "stable"], items[0].ExerciseParameters.Observations.Select(item => item.Comment));
        Assert.Equal((50m, 30, 8), (items[0].ExerciseParameters.LastWeight,
            items[0].ExerciseParameters.LastDurationInSeconds, items[0].ExerciseParameters.LastRepetitions));
        Assert.Empty(items[1].ExerciseParameters.Observations);
        Assert.Null(missing);
    }

    [Fact]
    public async Task ListAll_WhenDalReturnsNull_ReturnsEmptyAndCachesEmptyValue()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.ListAll().Returns((IEnumerable<RoutineFullDetailsDTO>?)null!);
        using MemoryCache cache = new(new MemoryCacheOptions());
        CachedRoutineRepository repository = new(new RoutineRepository(dal), cache);

        Assert.Empty(await repository.ListAll());
        Assert.Empty(await repository.ListAll());

        await dal.Received(1).ListAll();
    }

    [Fact]
    public async Task CreateAndUpdate_InvalidateCacheAndDelegateMappedItems()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.ListAll().Returns([CreateRow(1, "Cached", 11, 1, string.Empty, null, null)]);
        List<RoutineItemWriteModel> createdItems = [];
        List<RoutineItemWriteModel> updatedItems = [];
        dal.Create("New", Arg.Do<IReadOnlyList<RoutineItemWriteModel>>(list => createdItems = list.ToList())).Returns(7);
        _ = dal.Update(7, "Changed", Arg.Do<IReadOnlyList<RoutineItemWriteModel>>(list => updatedItems = list.ToList()));
        using MemoryCache cache = new(new MemoryCacheOptions());
        CachedRoutineRepository repository = new(new RoutineRepository(dal), cache);
        List<RoutineItem> items =
        [
            RoutineItem.Create(1, ExerciseParameters.FromDatabase(11, "E1", "", "", 3, (8, 12), 0, null, (60, 90), null, null, null, ExerciseTypes.WEIGHT, []), false, 1),
            RoutineItem.Create(2, ExerciseParameters.FromDatabase(12, "E2", "", "", 2, (1, 1), 30, null, (30, 0), null, null, null, ExerciseTypes.DURATION, []), true, 2)
        ];
        Routine routine = Routine.Create("New", items);

        await repository.ListAll();
        int id = await repository.Create(routine);
        await repository.ListAll();
        Routine updateRoutine = Routine.FromDatabase(7, "Changed", items);
        await repository.Update(updateRoutine);
        await repository.ListAll();

        Assert.Equal(7, id);
        await dal.Received(3).ListAll();
        await dal.Received(1).Create("New", Arg.Any<IReadOnlyList<RoutineItemWriteModel>>());
        await dal.Received(1).Update(7, "Changed", Arg.Any<IReadOnlyList<RoutineItemWriteModel>>());
        Assert.Equal([11, 12], createdItems.Select(item => item.ExerciseParametersId));
        Assert.Equal((ExerciseTypes.WEIGHT, 8, 12, (int?)null), (createdItems[0].Type, createdItems[0].RepetitionsMin, createdItems[0].RepetitionsMax, createdItems[0].Duration));
        Assert.Equal((ExerciseTypes.DURATION, 0, 0, (int?)30), (createdItems[1].Type, createdItems[1].RepetitionsMin, createdItems[1].RepetitionsMax, createdItems[1].Duration));
        Assert.Equal(createdItems.Count, updatedItems.Count);
    }

    [Fact]
    public async Task RoutineRepository_GetById_DirectlyCallsDalListById()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.ListById(1).Returns([CreateRow(1, "Direct", 11, 1, string.Empty, null, null)]);
        RoutineRepository repository = new(dal);

        Routine? found = await repository.GetById(1);

        await dal.Received(1).ListById(1, cancellationToken: Arg.Any<CancellationToken>());
        Assert.NotNull(found);
        Assert.Equal("Direct", found.Name);
    }

    [Fact]
    public async Task RoutineProjections_MapAndCacheDirectly()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.ListAll().Returns([CreateRow(1, "Direct", 11, 1, string.Empty, null, null)]);
        dal.ListById(1).Returns([CreateRow(1, "Direct", 11, 1, string.Empty, null, null)]);
        using MemoryCache cache = new(new MemoryCacheOptions());
        CachedRoutineRepository repository = new(new RoutineRepository(dal), cache);

        var list = await repository.ListRoutineProjections();
        var cachedList = await repository.ListRoutineProjections();
        var item = await repository.GetRoutineProjection(1);

        Assert.Single(list);
        Assert.Single(cachedList);
        Assert.NotNull(item);
        Assert.Equal("Direct", item.Name);
        await dal.Received(1).ListAll();
    }

    [Fact]
    public async Task GetById_CachesIndividually_AndDoesNotDumpAllRoutines()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.ListById(5).Returns([CreateRow(5, "Individual", 11, 1, string.Empty, null, null)]);
        using MemoryCache cache = new(new MemoryCacheOptions());
        CachedRoutineRepository repository = new(new RoutineRepository(dal), cache);

        Routine? first = await repository.GetById(5);
        Routine? second = await repository.GetById(5);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal("Individual", first.Name);
        Assert.Equal("Individual", second.Name);
        await dal.Received(1).ListById(5, cancellationToken: Arg.Any<CancellationToken>());
        await dal.DidNotReceive().ListAll(Arg.Any<int?>(), Arg.Any<CancellationToken>());
        Assert.True(cache.TryGetValue("Routine_5", out _));
    }

    [Fact]
    public async Task GetRoutineProjection_CachesIndividually_AndDoesNotDumpAllRoutines()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.ListById(5).Returns([CreateRow(5, "Projection", 11, 1, string.Empty, null, null)]);
        using MemoryCache cache = new(new MemoryCacheOptions());
        CachedRoutineRepository repository = new(new RoutineRepository(dal), cache);

        var first = await repository.GetRoutineProjection(5);
        var second = await repository.GetRoutineProjection(5);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal("Projection", first.Name);
        Assert.Equal("Projection", second.Name);
        await dal.Received(1).ListById(5, cancellationToken: Arg.Any<CancellationToken>());
        await dal.DidNotReceive().ListAll(Arg.Any<int?>(), Arg.Any<CancellationToken>());
        Assert.True(cache.TryGetValue("RoutineProjection_5", out _));
    }

    [Fact]
    public async Task CreateAndUpdate_InvalidateIndividualRoutineAndProjectionCaches()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.Create(Arg.Any<string>(), Arg.Any<IReadOnlyList<RoutineItemWriteModel>>(), Arg.Any<int?>(), Arg.Any<CancellationToken>()).Returns(10);
        using MemoryCache cache = new(new MemoryCacheOptions());
        CachedRoutineRepository repository = new(new RoutineRepository(dal), cache);

        cache.Set("Routine_10", Routine.FromDatabase(10, "Existing", []));
        cache.Set("RoutineProjection_10", new RoutineProjection { Id = 10, Name = "Existing" });

        Routine routineToCreate = Routine.FromDatabase(10, "New", []);
        await repository.Create(routineToCreate);

        Assert.False(cache.TryGetValue("Routine_10", out _));
        Assert.False(cache.TryGetValue("RoutineProjection_10", out _));

        cache.Set("Routine_10", Routine.FromDatabase(10, "Existing", []));
        cache.Set("RoutineProjection_10", new RoutineProjection { Id = 10, Name = "Existing" });

        Routine routineToUpdate = Routine.FromDatabase(10, "Updated", []);
        await repository.Update(routineToUpdate);

        Assert.False(cache.TryGetValue("Routine_10", out _));
        Assert.False(cache.TryGetValue("RoutineProjection_10", out _));
    }

    [Fact]
    public async Task RoutineRepository_GetById_WithUserId_ForwardsUserIdToDal()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.ListById(1, 42, Arg.Any<CancellationToken>()).Returns([CreateRow(1, "UserRoutine", 11, 1, string.Empty, null, null)]);
        RoutineRepository repository = new(dal);

        Routine? found = await repository.GetById(1, 42);

        await dal.Received(1).ListById(1, 42, Arg.Any<CancellationToken>());
        Assert.NotNull(found);
        Assert.Equal("UserRoutine", found.Name);
    }

    [Fact]
    public async Task RoutineRepository_GetRoutineProjection_WithUserId_ForwardsUserIdToDal()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.ListById(1, 42, Arg.Any<CancellationToken>()).Returns([CreateRow(1, "UserProjection", 11, 1, string.Empty, null, null)]);
        RoutineRepository repository = new(dal);

        RoutineProjection? found = await repository.GetRoutineProjection(1, 42);

        await dal.Received(1).ListById(1, 42, Arg.Any<CancellationToken>());
        Assert.NotNull(found);
        Assert.Equal("UserProjection", found.Name);
    }

    [Fact]
    public async Task CachedRoutineRepository_GetById_WithUserId_UsesDistinctCacheKeyPerUser()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.ListById(5, 10, Arg.Any<CancellationToken>()).Returns([CreateRow(5, "User10", 11, 1, string.Empty, null, null)]);
        dal.ListById(5, 20, Arg.Any<CancellationToken>()).Returns([CreateRow(5, "User20", 11, 1, string.Empty, null, null)]);
        using MemoryCache cache = new(new MemoryCacheOptions());
        CachedRoutineRepository repository = new(new RoutineRepository(dal), cache);

        Routine? user10First = await repository.GetById(5, 10);
        Routine? user10Second = await repository.GetById(5, 10);
        Routine? user20First = await repository.GetById(5, 20);

        Assert.Equal("User10", user10First?.Name);
        Assert.Equal("User10", user10Second?.Name);
        Assert.Equal("User20", user20First?.Name);
        await dal.Received(1).ListById(5, 10, Arg.Any<CancellationToken>());
        await dal.Received(1).ListById(5, 20, Arg.Any<CancellationToken>());
        Assert.True(cache.TryGetValue("Routine_5_10", out _));
        Assert.True(cache.TryGetValue("Routine_5_20", out _));
    }

    [Fact]
    public async Task CachedRoutineRepository_GetRoutineProjection_WithUserId_UsesDistinctCacheKeyPerUser()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.ListById(5, 10, Arg.Any<CancellationToken>()).Returns([CreateRow(5, "Proj10", 11, 1, string.Empty, null, null)]);
        dal.ListById(5, 20, Arg.Any<CancellationToken>()).Returns([CreateRow(5, "Proj20", 11, 1, string.Empty, null, null)]);
        using MemoryCache cache = new(new MemoryCacheOptions());
        CachedRoutineRepository repository = new(new RoutineRepository(dal), cache);

        RoutineProjection? proj10First = await repository.GetRoutineProjection(5, 10);
        RoutineProjection? proj10Second = await repository.GetRoutineProjection(5, 10);
        RoutineProjection? proj20First = await repository.GetRoutineProjection(5, 20);

        Assert.Equal("Proj10", proj10First?.Name);
        Assert.Equal("Proj10", proj10Second?.Name);
        Assert.Equal("Proj20", proj20First?.Name);
        await dal.Received(1).ListById(5, 10, Arg.Any<CancellationToken>());
        await dal.Received(1).ListById(5, 20, Arg.Any<CancellationToken>());
        Assert.True(cache.TryGetValue("RoutineProjection_5_10", out _));
        Assert.True(cache.TryGetValue("RoutineProjection_5_20", out _));
    }

    [Fact]
    public async Task CachedRoutineRepository_CreateAndUpdate_InvalidatesBothUserIdAndNonUserIdKeys()
    {
        IRoutineDAL dal = Substitute.For<IRoutineDAL>();
        dal.Create(Arg.Any<string>(), Arg.Any<IReadOnlyList<RoutineItemWriteModel>>(), Arg.Any<int?>(), Arg.Any<CancellationToken>()).Returns(5);
        using MemoryCache cache = new(new MemoryCacheOptions());
        CachedRoutineRepository repository = new(new RoutineRepository(dal), cache);

        cache.Set("Routine_5", Routine.FromDatabase(5, "R5", []));
        cache.Set("Routine_5_10", Routine.FromDatabase(5, "R5_10", []));
        cache.Set("RoutineProjection_5", new RoutineProjection { Id = 5, Name = "P5" });
        cache.Set("RoutineProjection_5_10", new RoutineProjection { Id = 5, Name = "P5_10" });

        Routine routine = Routine.FromDatabase(5, "Updated", [], userId: 10);
        await repository.Update(routine);

        Assert.False(cache.TryGetValue("Routine_5", out _));
        Assert.False(cache.TryGetValue("Routine_5_10", out _));
        Assert.False(cache.TryGetValue("RoutineProjection_5", out _));
        Assert.False(cache.TryGetValue("RoutineProjection_5_10", out _));
    }

    private static RoutineFullDetailsDTO CreateRow(
        int routineId, string routineName, int exerciseId, int position, string observations,
        decimal? lastWeight, int? lastRepetitions) => new()
    {
        RoutineId = routineId,
        RoutineName = routineName,
        RoutineItemId = exerciseId + 100,
        DayOfWeek = 2,
        ExerciseParametersId = exerciseId,
        MinRestTimeInSeconds = 60,
        MaxRestTimeInSeconds = 90,
        AlternatingSeries = true,
        Active = true,
        Position = position,
        ExerciseName = $"Exercise {exerciseId}",
        Description = "Description",
        Pattern = "Pattern",
        Series = 3,
        RepetitionsMin = 8,
        RepetitionsMax = 12,
        Duration = null,
        ReplaysInReserve = 2,
        TypeId = (int)ExerciseTypes.WEIGHT,
        LastWeight = lastWeight,
        LastDuration = 30,
        LastRepetitions = lastRepetitions,
        LastObservations = observations
    };
}
