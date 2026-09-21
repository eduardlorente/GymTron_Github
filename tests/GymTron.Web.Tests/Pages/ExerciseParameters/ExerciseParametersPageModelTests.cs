using FluentValidation;
using FluentValidation.Results;
using GymTron.Application.ExerciseParameters.Queries.DTO;
using GymTron.Domain.Enums;
using GymTron.Domain.Exceptions;
using GymTron.Web.Pages.ExerciseParameters;
using GymTron.Web.Services.Api;
using GymTron.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GymTron.Web.Tests.Pages.ExerciseParameters;

public class ExerciseParametersPageModelTests
{
    private readonly IGymTronWebApiClient _apiClient = Substitute.For<IGymTronWebApiClient>();

    [Fact]
    public async Task IndexModel_OnGetAsync_PopulatesExercisesFromQuery()
    {
        // Arrange
        var exerciseDtos = new List<ExerciseParameterDto>
        {
            new()
            {
                Id = 1,
                Name = "Squat",
                Description = "Barbell back squat",
                Pattern = "Squat",
                Type = ExerciseTypes.WEIGHT,
                ReplaysInReserve = 2
            },
            new()
            {
                Id = 2,
                Name = "Plank",
                Description = "Core endurance",
                Pattern = "Core",
                Type = ExerciseTypes.DURATION,
                ReplaysInReserve = 1
            }
        };

        _apiClient.GetExerciseParametersAsync(Arg.Any<CancellationToken>())
            .Returns(exerciseDtos);

        var model = new IndexModel(_apiClient);

        // Act
        await model.OnGetAsync();

        // Assert
        Assert.Equal(2, model.Exercises.Count);
        Assert.Equal("Squat", model.Exercises[0].Name);
        Assert.Equal("Squat", model.Exercises[0].Pattern);
        Assert.Equal("Plank", model.Exercises[1].Name);
        Assert.Equal(ExerciseTypes.DURATION, model.Exercises[1].Type);
    }

    [Fact]
    public async Task CreateModel_OnPostAsync_WhenModelStateInvalid_ReturnsPageWithoutSendingCommand()
    {
        // Arrange
        var model = new CreateModel(_apiClient);
        model.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        await _apiClient.DidNotReceive().CreateExerciseParameterAsync(Arg.Any<CreateExerciseParameterRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateModel_OnPostAsync_WhenValid_SendsCommandAndRedirectsToIndex()
    {
        // Arrange
        var model = new CreateModel(_apiClient)
        {
            Exercise = new ExerciseParameterEditViewModel
            {
                Name = "Bench Press",
                Description = "Chest press",
                Pattern = "Horizontal Push",
                Type = ExerciseTypes.WEIGHT,
                ReplaysInReserve = 2
            }
        };

        _apiClient.CreateExerciseParameterAsync(Arg.Any<CreateExerciseParameterRequest>(), Arg.Any<CancellationToken>())
            .Returns(10);

        // Act
        var result = await model.OnPostAsync();

        // Assert
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("Index", redirect.PageName);
        await _apiClient.Received(1).CreateExerciseParameterAsync(
            Arg.Is<CreateExerciseParameterRequest>(c => c.Name == "Bench Press" && c.Type == ExerciseTypes.WEIGHT),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EditModel_OnGetAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        _apiClient.GetExerciseParameterByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((ExerciseParameterDto?)null);

        var model = new EditModel(_apiClient);

        // Act
        var result = await model.OnGetAsync(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditModel_OnGetAsync_WhenFound_PopulatesViewModelAndReturnsPage()
    {
        // Arrange
        var dto = new ExerciseParameterDto
        {
            Id = 5,
            Name = "Overhead Press",
            Description = "Shoulder press",
            Pattern = "Vertical Push",
            Type = ExerciseTypes.WEIGHT,
            ReplaysInReserve = 1
        };

        _apiClient.GetExerciseParameterByIdAsync(5, Arg.Any<CancellationToken>())
            .Returns(dto);

        var model = new EditModel(_apiClient);

        // Act
        var result = await model.OnGetAsync(5);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal(5, model.Exercise.Id);
        Assert.Equal("Overhead Press", model.Exercise.Name);
    }

    [Fact]
    public async Task EditModel_OnPostAsync_WhenModelStateInvalid_ReturnsPageWithoutSendingCommand()
    {
        // Arrange
        var model = new EditModel(_apiClient);
        model.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await model.OnPostAsync(5);

        // Assert
        Assert.IsType<PageResult>(result);
        await _apiClient.DidNotReceive().UpdateExerciseParameterAsync(Arg.Any<int>(), Arg.Any<UpdateExerciseParameterRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EditModel_OnPostAsync_WhenValid_SendsCommandAndRedirectsToIndex()
    {
        // Arrange
        var model = new EditModel(_apiClient)
        {
            Exercise = new ExerciseParameterEditViewModel
            {
                Id = 5,
                Name = "Overhead Press Updated",
                Description = "Standing shoulder press",
                Pattern = "Vertical Push",
                Type = ExerciseTypes.WEIGHT,
                ReplaysInReserve = 2
            }
        };

        // Act
        var result = await model.OnPostAsync(5);

        // Assert
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("Index", redirect.PageName);
        await _apiClient.Received(1).UpdateExerciseParameterAsync(
            5,
            Arg.Is<UpdateExerciseParameterRequest>(c => c.Name == "Overhead Press Updated"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateModel_OnPostAsync_WhenValidationException_AddsErrorsToModelStateAndReturnsPage()
    {
        // Arrange
        var model = new CreateModel(_apiClient)
        {
            Exercise = new ExerciseParameterEditViewModel { Name = "" }
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("Exercise.Name", "Name is required")
        };
        _apiClient.CreateExerciseParameterAsync(Arg.Any<CreateExerciseParameterRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ValidationException(validationFailures));

        // Act
        var result = await model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey("Exercise.Name"));
    }

    [Fact]
    public async Task CreateModel_OnPostAsync_WhenDomainException_AddsErrorToModelStateAndReturnsPage()
    {
        // Arrange
        var model = new CreateModel(_apiClient)
        {
            Exercise = new ExerciseParameterEditViewModel { Name = "Existing" }
        };

        _apiClient.CreateExerciseParameterAsync(Arg.Any<CreateExerciseParameterRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidDomainOperationException("Exercise already exists."));

        // Act
        var result = await model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey(string.Empty));
    }

    [Fact]
    public async Task EditModel_OnPostAsync_WhenValidationException_AddsErrorsToModelStateAndReturnsPage()
    {
        // Arrange
        var model = new EditModel(_apiClient)
        {
            Exercise = new ExerciseParameterEditViewModel { Id = 5, Name = "" }
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("Exercise.Name", "Name is required")
        };
        _apiClient.UpdateExerciseParameterAsync(5, Arg.Any<UpdateExerciseParameterRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ValidationException(validationFailures));

        // Act
        var result = await model.OnPostAsync(5);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey("Exercise.Name"));
    }

    [Fact]
    public async Task EditModel_OnPostAsync_WhenDomainException_AddsErrorToModelStateAndReturnsPage()
    {
        // Arrange
        var model = new EditModel(_apiClient)
        {
            Exercise = new ExerciseParameterEditViewModel { Id = 5, Name = "Existing" }
        };

        _apiClient.UpdateExerciseParameterAsync(5, Arg.Any<UpdateExerciseParameterRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new EntityNotFoundException("Exercise parameter not found."));

        // Act
        var result = await model.OnPostAsync(5);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey(string.Empty));
    }
}
