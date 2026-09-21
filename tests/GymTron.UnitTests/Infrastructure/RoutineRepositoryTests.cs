using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
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
        using MemoryCache cache = new(new MemoryCacheOptions());
        CachedRoutineRepository repository = new(new RoutineRepository(dal), cache);

        List<GymTron.Domain.Entities.Routine> routines = await repository.ListAll();
        GymTron.Domain.Entities.Routine? found = await repository.GetById(1);
        GymTron.Domain.Entities.Routine? missing = await repository.GetById(99);

        await dal.Received(1).ListAll();
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

        await dal.Received(1).ListById(1, Arg.Any<CancellationToken>());
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
