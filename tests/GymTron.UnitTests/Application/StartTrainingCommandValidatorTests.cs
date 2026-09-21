using GymTron.Application.Trainings.Commands;

namespace GymTron.UnitTests.Application;

public class StartTrainingCommandValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveRoutineId_IsInvalid(int routineId)
    {
        StartTrainingCommand command = new(Guid.NewGuid(), routineId, 1);
        StartTrainingCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(StartTrainingCommand.RoutineId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(-1)]
    public void Validate_WithInvalidDayOfWeek_IsInvalid(int dayOfWeek)
    {
        StartTrainingCommand command = new(Guid.NewGuid(), 1, dayOfWeek);
        StartTrainingCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(StartTrainingCommand.DayOfWeek));
    }

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        StartTrainingCommand command = new(Guid.NewGuid(), 1, 1);
        StartTrainingCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
