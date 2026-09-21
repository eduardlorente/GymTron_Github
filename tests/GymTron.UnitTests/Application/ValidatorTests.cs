using GymTron.Application.Routines.Commands;
using GymTron.Application.Routines.Queries;
using GymTron.Application.ExerciseParameters.Commands;
using GymTron.Application.ExerciseParameters.Queries;
using GymTron.Domain.Enums;

namespace GymTron.UnitTests.Application;

public class ValidatorTests
{
    [Fact]
    public void CreateRoutineCommand_WithValidData_IsValid()
    {
        CreateRoutineCommand command = new(Guid.NewGuid(), "Strength", [CreateValidRoutineItem()]);
        CreateRoutineCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateRoutineCommand_WithInvalidName_IsInvalid(string? name)
    {
        CreateRoutineCommand command = new(Guid.NewGuid(), name!, [CreateValidRoutineItem()]);
        CreateRoutineCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRoutineCommand.Name));
    }

    [Fact]
    public void CreateRoutineCommand_WithEmptyItems_IsInvalid()
    {
        CreateRoutineCommand command = new(Guid.NewGuid(), "Strength", []);
        CreateRoutineCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRoutineCommand.Items));
    }

    [Fact]
    public void UpdateRoutineCommand_WithValidData_IsValid()
    {
        UpdateRoutineCommand command = new(Guid.NewGuid(), 1, "Strength", [CreateValidRoutineItem()]);
        UpdateRoutineCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateRoutineCommand_WithInvalidId_IsInvalid(int id)
    {
        UpdateRoutineCommand command = new(Guid.NewGuid(), id, "Strength", [CreateValidRoutineItem()]);
        UpdateRoutineCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRoutineCommand.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(-1)]
    public void RoutineItemInput_WithInvalidDayOfWeek_IsInvalid(int dayOfWeek)
    {
        RoutineItemInput item = CreateValidRoutineItem();
        item.DayOfWeek = dayOfWeek;
        CreateRoutineCommand command = new(Guid.NewGuid(), "Strength", [item]);
        CreateRoutineCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RoutineItemInput_WithInvalidExerciseParametersId_IsInvalid(int exerciseParametersId)
    {
        RoutineItemInput item = CreateValidRoutineItem();
        item.ExerciseParametersId = exerciseParametersId;
        CreateRoutineCommand command = new(Guid.NewGuid(), "Strength", [item]);
        CreateRoutineCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RoutineItemInput_WithInvalidSeries_IsInvalid(int series)
    {
        RoutineItemInput item = CreateValidRoutineItem();
        item.Series = series;
        CreateRoutineCommand command = new(Guid.NewGuid(), "Strength", [item]);
        CreateRoutineCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateExerciseParameterCommand_WithValidData_IsValid()
    {
        CreateExerciseParameterCommand command = new(Guid.NewGuid(), "Squat", "Description", "Pattern", ExerciseTypes.WEIGHT, 2);
        CreateExerciseParameterCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateExerciseParameterCommand_WithInvalidName_IsInvalid(string? name)
    {
        CreateExerciseParameterCommand command = new(Guid.NewGuid(), name!, "Description", "Pattern", ExerciseTypes.WEIGHT, 2);
        CreateExerciseParameterCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateExerciseParameterCommand.Name));
    }

    [Fact]
    public void CreateExerciseParameterCommand_WithNegativeReplaysInReserve_IsInvalid()
    {
        CreateExerciseParameterCommand command = new(Guid.NewGuid(), "Squat", "Description", "Pattern", ExerciseTypes.WEIGHT, -1);
        CreateExerciseParameterCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateExerciseParameterCommand.ReplaysInReserve));
    }

    [Fact]
    public void UpdateExerciseParameterCommand_WithValidData_IsValid()
    {
        UpdateExerciseParameterCommand command = new(Guid.NewGuid(), 1, "Squat", "Description", "Pattern", ExerciseTypes.WEIGHT, 2);
        UpdateExerciseParameterCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateExerciseParameterCommand_WithInvalidId_IsInvalid(int id)
    {
        UpdateExerciseParameterCommand command = new(Guid.NewGuid(), id, "Squat", "Description", "Pattern", ExerciseTypes.WEIGHT, 2);
        UpdateExerciseParameterCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateExerciseParameterCommand.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetRoutineByIdQuery_WithInvalidRoutineId_IsInvalid(int routineId)
    {
        GetRoutineByIdQuery query = new(Guid.NewGuid(), routineId);
        GetRoutineByIdQueryValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetRoutineByIdQuery.RoutineId));
    }

    [Fact]
    public void GetRoutineByIdQuery_WithValidRoutineId_IsValid()
    {
        GetRoutineByIdQuery query = new(Guid.NewGuid(), 1);
        GetRoutineByIdQueryValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetExerciseParameterByIdQuery_WithInvalidId_IsInvalid(int id)
    {
        GetExerciseParameterByIdQuery query = new(Guid.NewGuid(), id);
        GetExerciseParameterByIdQueryValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetExerciseParameterByIdQuery.Id));
    }

    [Fact]
    public void GetExerciseParameterByIdQuery_WithValidId_IsValid()
    {
        GetExerciseParameterByIdQuery query = new(Guid.NewGuid(), 1);
        GetExerciseParameterByIdQueryValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("ValidPass123", true)]
    [InlineData("Short1A", false)]
    [InlineData("alllowercase123", false)]
    [InlineData("ALLUPPERCASE123", false)]
    [InlineData("NoDigitsInPassword", false)]
    public void RegisterUserCommandValidator_PasswordRules_ValidatedCorrectly(string password, bool expectedValid)
    {
        GymTron.Application.Auth.Commands.Register.RegisterUserCommand command =
            new(Guid.NewGuid(), "validUser", "user@gymtron.com", password);
        GymTron.Application.Auth.Commands.Register.RegisterUserCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.Equal(expectedValid, result.IsValid);
        if (!expectedValid)
        {
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(GymTron.Application.Auth.Commands.Register.RegisterUserCommand.Password));
        }
    }

    private static RoutineItemInput CreateValidRoutineItem() => new()
    {
        DayOfWeek = 1,
        ExerciseParametersId = 1,
        Series = 3,
        RepetitionsMin = 8,
        RepetitionsMax = 12,
        MinRestTimeInSeconds = 60,
        Position = 1,
        Type = ExerciseTypes.WEIGHT
    };
}
