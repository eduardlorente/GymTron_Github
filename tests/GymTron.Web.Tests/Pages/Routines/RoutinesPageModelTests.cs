using FluentValidation;
using FluentValidation.Results;
using GymTron.Application.ExerciseParameters.Queries.DTO;
using GymTron.Application.Routines.Queries.DTO;
using GymTron.Domain.Exceptions;
using GymTron.Web.Pages.Routines;
using GymTron.Web.Services.Api;
using GymTron.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GymTron.Web.Tests.Pages.Routines;

public class RoutinesPageModelTests
{
    private readonly IGymTronWebApiClient _apiClient = Substitute.For<IGymTronWebApiClient>();

    [Fact]
    public async Task IndexModel_OnGetAsync_PopulatesRoutinesFromQuery()
    {
        // Arrange
        var routineDtos = new List<RoutineDto>
        {
            new()
            {
                Id = 1,
                Name = "Hypertrophy Push",
                Items = new List<RoutineItemDto>
                {
                    new()
                    {
                        Id = 10,
                        DayOfWeek = 1,
                        ExerciseParametersId = 5,
                        ExerciseName = "Bench Press",
                        Series = 4,
                        RepetitionsMin = 8,
                        RepetitionsMax = 12,
                        MinRestTimeInSeconds = 90,
                        MaxRestTimeInSeconds = 120,
                        AlternatingSeries = false
                    }
                }
            }
        };

        _apiClient.GetRoutinesAsync(Arg.Any<CancellationToken>())
            .Returns(routineDtos);

        var model = new IndexModel(_apiClient);

        // Act
        await model.OnGetAsync();

        // Assert
        Assert.Single(model.Routines);
        Assert.Equal(1, model.Routines[0].Id);
        Assert.Equal("Hypertrophy Push", model.Routines[0].Name);
        Assert.Single(model.Routines[0].Items);
        Assert.Equal("Bench Press", model.Routines[0].Items[0].ExerciseName);
    }

    [Fact]
    public async Task DetailsModel_OnGetAsync_WhenNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        _apiClient.GetRoutineByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((RoutineDto?)null);

        var model = new DetailsModel(_apiClient);

        // Act
        var result = await model.OnGetAsync(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DetailsModel_OnGetAsync_WhenFound_ReturnsPageResultAndPopulatesModel()
    {
        // Arrange
        var routineDto = new RoutineDto
        {
            Id = 42,
            Name = "Pull Day",
            Items = new List<RoutineItemDto>
            {
                new()
                {
                    Id = 100,
                    DayOfWeek = 2,
                    ExerciseParametersId = 8,
                    ExerciseName = "Pull-up",
                    Series = 3,
                    RepetitionsMin = 6,
                    RepetitionsMax = 10,
                    MinRestTimeInSeconds = 60,
                    MaxRestTimeInSeconds = 90,
                    AlternatingSeries = true
                }
            }
        };

        _apiClient.GetRoutineByIdAsync(42, Arg.Any<CancellationToken>())
            .Returns(routineDto);

        var model = new DetailsModel(_apiClient);

        // Act
        var result = await model.OnGetAsync(42);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal(42, model.Routine.Id);
        Assert.Equal("Pull Day", model.Routine.Name);
        Assert.Single(model.Routine.Items);
        Assert.Equal("Pull-up", model.Routine.Items[0].ExerciseName);
    }

    [Fact]
    public async Task CreateModel_OnPostAsync_WhenValidationException_AddsErrorsToModelStateAndReturnsPage()
    {
        // Arrange
        _apiClient.GetExerciseParametersAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseParameterDto>());

        var model = new CreateModel(_apiClient)
        {
            Routine = new RoutineEditViewModel { Name = "" }
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("Routine.Name", "Name is required")
        };
        _apiClient.CreateRoutineAsync(Arg.Any<CreateRoutineRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ValidationException(validationFailures));

        // Act
        var result = await model.OnPostAsync(null, null);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey("Routine.Name"));
    }

    [Fact]
    public async Task CreateModel_OnPostAsync_WhenDomainException_AddsErrorToModelStateAndReturnsPage()
    {
        // Arrange
        _apiClient.GetExerciseParametersAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseParameterDto>());

        var model = new CreateModel(_apiClient)
        {
            Routine = new RoutineEditViewModel { Name = "Existing Routine" }
        };

        _apiClient.CreateRoutineAsync(Arg.Any<CreateRoutineRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidDomainOperationException("Routine name already exists."));

        // Act
        var result = await model.OnPostAsync(null, null);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey(string.Empty));
    }

    [Fact]
    public async Task EditModel_OnPostAsync_WhenValidationException_AddsErrorsToModelStateAndReturnsPage()
    {
        // Arrange
        _apiClient.GetExerciseParametersAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseParameterDto>());

        var model = new EditModel(_apiClient)
        {
            Routine = new RoutineEditViewModel { Id = 1, Name = "" }
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("Routine.Name", "Name is required")
        };
        _apiClient.UpdateRoutineAsync(1, Arg.Any<UpdateRoutineRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ValidationException(validationFailures));

        // Act
        var result = await model.OnPostAsync(1, null, null, null);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey("Routine.Name"));
    }

    [Fact]
    public async Task EditModel_OnPostAsync_WhenDomainException_AddsErrorToModelStateAndReturnsPage()
    {
        // Arrange
        _apiClient.GetExerciseParametersAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseParameterDto>());

        var model = new EditModel(_apiClient)
        {
            Routine = new RoutineEditViewModel { Id = 1, Name = "Existing" }
        };

        _apiClient.UpdateRoutineAsync(1, Arg.Any<UpdateRoutineRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new EntityNotFoundException("Routine not found."));

        // Act
        var result = await model.OnPostAsync(1, null, null, null);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey(string.Empty));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(0)]
    public void EditModel_GetDayName_HandlesInputsGracefully(int dayOfWeek)
    {
        // Arrange
        var model = new EditModel(_apiClient);

        // Act
        var result = model.GetDayName(dayOfWeek);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result));
    }
}
