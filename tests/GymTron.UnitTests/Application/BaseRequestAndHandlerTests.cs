using GymTron.Application.Base;
using GymTron.Domain.Services;
using MediatR;
using NSubstitute;

namespace GymTron.UnitTests.Application;

public class BaseRequestAndHandlerTests
{
    [Fact]
    public void Requests_WithCorrelationId_PreserveItAndRejectEmptyValues()
    {
        Guid correlationId = Guid.NewGuid();

        Assert.Equal(correlationId, new CommandBase(correlationId).CorrelationId);
        Assert.Equal(correlationId, new CommandBaseWithResponse<int>(correlationId).CorrelationId);
        Assert.Equal(correlationId, new QueryBase<int>(correlationId).CorrelationId);
        Assert.Throws<ArgumentException>(() => new CommandBase(Guid.Empty));
        Assert.Throws<ArgumentException>(() => new CommandBaseWithResponse<int>(Guid.Empty));
        Assert.Throws<ArgumentException>(() => new QueryBase<int>(Guid.Empty));
    }

    [Fact]
    public void PaginatedQuery_Constructors_PreservePagingAndOptionalSorting()
    {
        TestPaginatedQuery defaultSort = new(Guid.NewGuid(), 2, 25);
        TestPaginatedQuery explicitSort = new(Guid.NewGuid(), 3, 10, "Name", false);

        Assert.Equal((2, 25, null, true), (defaultSort.Page, defaultSort.PageSize, defaultSort.SortBy, defaultSort.SortAscending));
        Assert.Equal((3, 10, "Name", false), (explicitSort.Page, explicitSort.PageSize, explicitSort.SortBy, explicitSort.SortAscending));
    }

    [Fact]
    public async Task BaseHandlers_OnSuccess_ReturnWithoutLogging()
    {
        IExceptionLogger<TestCommand> commandLogger = Substitute.For<IExceptionLogger<TestCommand>>();
        IExceptionLogger<TestResponseCommand> responseLogger = Substitute.For<IExceptionLogger<TestResponseCommand>>();
        IExceptionLogger<TestQuery> queryLogger = Substitute.For<IExceptionLogger<TestQuery>>();

        await new TestCommandHandler(commandLogger).Handle(new TestCommand(), CancellationToken.None);
        int commandResult = await new TestResponseCommandHandler(responseLogger).Handle(new TestResponseCommand(), CancellationToken.None);
        int queryResult = await new TestQueryHandler(queryLogger).Handle(new TestQuery(), CancellationToken.None);

        Assert.Equal(17, commandResult);
        Assert.Equal(23, queryResult);
        await commandLogger.DidNotReceive().LogException(Arg.Any<Exception>());
        await responseLogger.DidNotReceive().LogException(Arg.Any<Exception>());
        await queryLogger.DidNotReceive().LogException(Arg.Any<Exception>());
    }

    [Fact]
    public async Task BaseHandlers_OnFailure_LogTheSameExceptionAndRethrow()
    {
        InvalidOperationException failure = new("failure");
        IExceptionLogger<TestCommand> commandLogger = Substitute.For<IExceptionLogger<TestCommand>>();
        IExceptionLogger<TestResponseCommand> responseLogger = Substitute.For<IExceptionLogger<TestResponseCommand>>();
        IExceptionLogger<TestQuery> queryLogger = Substitute.For<IExceptionLogger<TestQuery>>();

        Exception commandException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new TestCommandHandler(commandLogger, failure).Handle(new TestCommand(), CancellationToken.None));
        Exception responseException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new TestResponseCommandHandler(responseLogger, failure).Handle(new TestResponseCommand(), CancellationToken.None));
        Exception queryException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new TestQueryHandler(queryLogger, failure).Handle(new TestQuery(), CancellationToken.None));

        Assert.Same(failure, commandException);
        Assert.Same(failure, responseException);
        Assert.Same(failure, queryException);
        await commandLogger.Received(1).LogException(failure);
        await responseLogger.Received(1).LogException(failure);
        await queryLogger.Received(1).LogException(failure);
    }

    private sealed class TestPaginatedQuery : PaginatedResultsQueryBase<int>
    {
        internal TestPaginatedQuery(Guid correlationId, int page, int pageSize) : base(correlationId, page, pageSize) { }
        internal TestPaginatedQuery(Guid correlationId, int page, int pageSize, string? sortBy, bool ascending)
            : base(correlationId, page, pageSize, sortBy, ascending) { }
    }

    public sealed class TestCommand : IRequest;
    public sealed class TestResponseCommand : IRequest<int>;
    public sealed class TestQuery : IRequest<int>;

    private sealed class TestCommandHandler(IExceptionLogger<TestCommand> logger, Exception? failure = null)
        : BaseCommandHandler<TestCommand>(logger)
    {
        protected override Task HandleCommand(TestCommand request, CancellationToken cancellationToken)
            => failure == null ? Task.CompletedTask : Task.FromException(failure);
    }

    private sealed class TestResponseCommandHandler(IExceptionLogger<TestResponseCommand> logger, Exception? failure = null)
        : BaseCommandHandlerWithResponse<TestResponseCommand, int>(logger)
    {
        protected override Task<int> HandleCommand(TestResponseCommand request, CancellationToken cancellationToken)
            => failure == null ? Task.FromResult(17) : Task.FromException<int>(failure);
    }

    private sealed class TestQueryHandler(IExceptionLogger<TestQuery> logger, Exception? failure = null)
        : BaseQueryHandler<TestQuery, int>(logger)
    {
        protected override Task<int> HandleQuery(TestQuery request, CancellationToken cancellationToken)
            => failure == null ? Task.FromResult(23) : Task.FromException<int>(failure);
    }
}
