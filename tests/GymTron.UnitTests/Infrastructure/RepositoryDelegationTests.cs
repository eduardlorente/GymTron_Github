using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.Infrastructure.Persistence.Repositories;
using NSubstitute;

namespace GymTron.UnitTests.Infrastructure;

public class RepositoryDelegationTests
{
    [Fact]
    public async Task BodyWeightRepository_DelegatesAddAndMapsListInSourceOrder()
    {
        IBodyWeightDAL dal = Substitute.For<IBodyWeightDAL>();
        DateTime firstDate = new(2026, 1, 1);
        DateTime secondDate = new(2026, 2, 1);
        dal.ListAll().Returns([
            new BodyWeightDALModel { Id = 2, Weight = 79, BodyFatPercentage = 15, CreatedOn = secondDate },
            new BodyWeightDALModel { Id = 1, Weight = 80, BodyFatPercentage = 16, CreatedOn = firstDate }
        ]);
        BodyWeightRepository repository = new(dal);
        BodyWeight added = BodyWeight.New(81, 17, firstDate);

        await repository.Add(added);
        List<BodyWeight> result = await repository.ListAll();

        await dal.Received(1).Add(added);
        Assert.Equal([2, 1], result.Select(item => item.Id));
        Assert.Equal((79m, 15m, secondDate),
            (result[0].Weight, result[0].BodyFatPercentage, result[0].Status.CreatedOn));
    }

    [Fact]
    public async Task ExerciseRepository_DelegatesWritesAndMapsListsIncludingObservationEdges()
    {
        IExerciseDAL dal = Substitute.For<IExerciseDAL>();
        DateTime createdOn = new(2026, 3, 1);
        ExerciseDALModel populated = new()
        {
            Id = 4, TrainingId = 5, ExerciseParametersId = 6, Name = "Row", Weight = 50,
            DurationInSeconds = 30, Repetitions = 8, CreatedOn = createdOn, ObservationsCSV = "steady;controlled"
        };
        ExerciseDALModel empty = new()
        {
            Id = 7, TrainingId = 8, ExerciseParametersId = 9, Name = "Plank", CreatedOn = createdOn,
            ObservationsCSV = string.Empty
        };
        dal.ListByTrainingId(5).Returns([populated]);
        dal.ListAll().Returns([empty, populated]);
        ExerciseRepository repository = new(dal);
        Exercise added = Exercise.New(5, 6, "Row", 50, 30, 8, ["steady"]);
        List<Exercise> range = [added];

        await repository.Add(added);
        await repository.AddRange(range);
        List<Exercise> byTraining = await repository.ListByTraining(5);
        List<Exercise> all = await repository.ListAll();

        await dal.Received(1).Add(added);
        await dal.Received(1).AddRange(range);
        Exercise mapped = Assert.Single(byTraining);
        Assert.Equal((4, 5, 6, "Row", 50m, 30, 8, createdOn),
            (mapped.Id, mapped.TrainingId, mapped.ExerciseParametersId, mapped.Name, mapped.Weight,
             mapped.DurationInSeconds, mapped.CurrentRepetitions, mapped.Status.CreatedOn));
        Assert.Equal(["steady", "controlled"], mapped.Observations.Select(item => item.Comment));
        Assert.Equal(2, all.Count);
        Assert.Empty(all[0].Observations);
    }

    [Fact]
    public async Task ExerciseParameterRepository_MapsNullEmptyAndPopulatedResultsAndDelegatesWrites()
    {
        IExerciseParameterDAL dal = Substitute.For<IExerciseParameterDAL>();
        ExerciseParameterDALModel model = new()
        {
            Id = 3, Name = "Squat", Description = "Description", Pattern = "Pattern",
            TypeId = (int)ExerciseTypes.WEIGHT, ReplaysInReserve = 2
        };
        dal.ListAll().Returns([model]);
        dal.GetById(3).Returns(model);
        dal.GetById(4).Returns((ExerciseParameterDALModel?)null);
        dal.Create("Squat", "Description", "Pattern", 1, 2).Returns(9);
        ExerciseParameterRepository repository = new(dal);

        List<ExerciseParameters> all = await repository.ListAll();
        ExerciseParameters? found = await repository.GetById(3);
        ExerciseParameters? missing = await repository.GetById(4);
        ExerciseParameters newParam = ExerciseParameters.Create("Squat", "Description", "Pattern", ExerciseTypes.WEIGHT, 2);
        int id = await repository.Create(newParam);
        ExerciseParameters updateParam = ExerciseParameters.FromDatabase(9, "Row", "D2", "P2", 0, (0, 0), 0, null, (0, 0), null, null, null, ExerciseTypes.DURATION, []);
        await repository.Update(updateParam);

        ExerciseParameters mapped = Assert.Single(all);
        Assert.Equal((3, "Squat", "Description", "Pattern", ExerciseTypes.WEIGHT, 2),
            (mapped.Id, mapped.Name, mapped.Description, mapped.Pattern, mapped.Type, mapped.ReplaysInReserve));
        Assert.Equal(mapped.Id, found!.Id);
        Assert.Null(missing);
        Assert.Equal(9, id);
        await dal.Received(1).Update(9, "Row", "D2", "P2", 2, null);
    }

    [Fact]
    public async Task Repositories_ListHistoryAndProjections_MapDirectlyWithoutAggregateConstruction()
    {
        IBodyWeightDAL bwDal = Substitute.For<IBodyWeightDAL>();
        DateTime d1 = new(2026, 1, 1);
        DateTime d2 = new(2026, 2, 1);
        bwDal.ListAll().Returns([
            new BodyWeightDALModel { Id = 1, Weight = 80, BodyFatPercentage = 16, CreatedOn = d1 },
            new BodyWeightDALModel { Id = 2, Weight = 79, BodyFatPercentage = 15, CreatedOn = d2 }
        ]);
        BodyWeightRepository bwRepo = new(bwDal);
        var bwHistory = await bwRepo.ListHistory();
        Assert.Equal([d2, d1], bwHistory.Select(b => b.CreatedOn));

        IExerciseDAL exDal = Substitute.For<IExerciseDAL>();
        exDal.ListAll().Returns([
            new ExerciseDALModel { Id = 1, Name = "Squat", Weight = 100, Repetitions = 5, DurationInSeconds = 0, CreatedOn = d1 },
            new ExerciseDALModel { Id = 2, Name = "Bench", Weight = 80, Repetitions = 8, DurationInSeconds = 0, CreatedOn = d2 }
        ]);
        ExerciseRepository exRepo = new(exDal);
        var exHistory = await exRepo.ListHistory();
        Assert.Equal(["Bench", "Squat"], exHistory.Select(e => e.Name));

        IExerciseParameterDAL epDal = Substitute.For<IExerciseParameterDAL>();
        ExerciseParameterDALModel epModel = new() { Id = 5, Name = "Deadlift", Description = "Hinge", Pattern = "Hinge", TypeId = (int)ExerciseTypes.WEIGHT, ReplaysInReserve = 1 };
        epDal.GetById(5).Returns(epModel);
        epDal.GetById(99).Returns((ExerciseParameterDALModel?)null);
        epDal.ListAll().Returns([epModel]);
        ExerciseParameterRepository epRepo = new(epDal);

        var epProj = await epRepo.GetProjection(5);
        var epMissing = await epRepo.GetProjection(99);
        var epList = await epRepo.ListProjections();

        Assert.NotNull(epProj);
        Assert.Equal((5, "Deadlift", ExerciseTypes.WEIGHT), (epProj.Id, epProj.Name, epProj.Type));
        Assert.Null(epMissing);
        Assert.Single(epList);
    }
}
