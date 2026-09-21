using FluentValidation;
using GymTron.Application.Base;
using GymTron.Application.Trainings.Commands;
using MediatR;
using NSubstitute;

namespace GymTron.UnitTests.Application;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithInvalidRequest_ThrowsValidationException()
    {
        IValidator<StartTrainingCommand> validator = Substitute.For<IValidator<StartTrainingCommand>>();
        FluentValidation.Results.ValidationResult validationResult = new(new[]
        {
            new FluentValidation.Results.ValidationFailure("RoutineId", "RoutineId must be greater than 0")
        });
        validator.ValidateAsync(Arg.Any<ValidationContext<StartTrainingCommand>>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);

        ValidationBehavior<StartTrainingCommand, Unit> behavior = new([validator]);
        StartTrainingCommand request = new(Guid.NewGuid(), 0, 1);
        RequestHandlerDelegate<Unit> next = () => Task.FromResult(Unit.Value);

        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(request, next, CancellationToken.None));

        Assert.Single(exception.Errors);
        Assert.Equal("RoutineId", exception.Errors.First().PropertyName);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CallsNext()
    {
        IValidator<StartTrainingCommand> validator = Substitute.For<IValidator<StartTrainingCommand>>();
        FluentValidation.Results.ValidationResult validationResult = new(Array.Empty<FluentValidation.Results.ValidationFailure>());
        validator.ValidateAsync(Arg.Any<ValidationContext<StartTrainingCommand>>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);

        ValidationBehavior<StartTrainingCommand, Unit> behavior = new([validator]);
        StartTrainingCommand request = new(Guid.NewGuid(), 1, 1);
        bool nextCalled = false;
        RequestHandlerDelegate<Unit> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Unit.Value);
        };

        await behavior.Handle(request, next, CancellationToken.None);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Handle_WithNoValidators_CallsNext()
    {
        ValidationBehavior<StartTrainingCommand, Unit> behavior = new([]);
        StartTrainingCommand request = new(Guid.NewGuid(), 1, 1);
        bool nextCalled = false;
        RequestHandlerDelegate<Unit> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Unit.Value);
        };

        await behavior.Handle(request, next, CancellationToken.None);

        Assert.True(nextCalled);
    }
}
