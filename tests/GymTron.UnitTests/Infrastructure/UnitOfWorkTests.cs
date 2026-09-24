using System.Data;
using System.Data.Common;
using GymTron.Infrastructure.Persistence;

namespace GymTron.UnitTests.Infrastructure;

public class UnitOfWorkTests
{
    private sealed class FakeDbTransaction : DbTransaction
    {
        public bool WasCommitted { get; private set; }
        public bool WasRolledBack { get; private set; }
        public bool WasDisposed { get; private set; }

        public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
        protected override DbConnection? DbConnection => null;

        public override void Commit() => WasCommitted = true;

        public override Task CommitAsync(CancellationToken cancellationToken = default)
        {
            WasCommitted = true;
            return Task.CompletedTask;
        }

        public override void Rollback() => WasRolledBack = true;

        public override Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            WasRolledBack = true;
            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            WasDisposed = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeDbConnection : DbConnection
    {
        private ConnectionState _state = ConnectionState.Closed;

        public FakeDbTransaction? LastTransaction { get; private set; }
        public bool WasDisposed { get; private set; }
        public bool FailOnOpen { get; set; }

        [System.Diagnostics.CodeAnalysis.AllowNull]
        public override string ConnectionString { get; set; } = string.Empty;
        public override string Database => "TestDb";
        public override string DataSource => "localhost";
        public override string ServerVersion => "8.0";
        public override ConnectionState State => _state;

        public override void ChangeDatabase(string databaseName) { }
        public override void Close() => _state = ConnectionState.Closed;
        public override void Open() => _state = ConnectionState.Open;

        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            if (FailOnOpen)
            {
                throw new InvalidOperationException("Failed to open connection.");
            }

            _state = ConnectionState.Open;
            return Task.CompletedTask;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            LastTransaction = new FakeDbTransaction();
            return LastTransaction;
        }

        protected override DbCommand CreateDbCommand() => throw new NotImplementedException();

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            _state = ConnectionState.Closed;
            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            WasDisposed = true;
            _state = ConnectionState.Closed;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConnectionStringIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new UnitOfWork((string)null!));
    }

    [Fact]
    public async Task BeginAsync_OpensConnectionAndBeginsTransaction_WhenNotStarted()
    {
        FakeDbConnection fakeConn = new();
        UnitOfWork uow = new(() => fakeConn);

        await uow.BeginAsync();

        Assert.Equal(ConnectionState.Open, fakeConn.State);
        Assert.True(uow.HasActiveTransaction);
        Assert.NotNull(uow.ActiveConnection);
        Assert.NotNull(uow.ActiveTransaction);
    }

    [Fact]
    public async Task BeginAsync_ThrowsInvalidOperationException_WhenAlreadyActive()
    {
        FakeDbConnection fakeConn = new();
        UnitOfWork uow = new(() => fakeConn);

        await uow.BeginAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => uow.BeginAsync());
    }

    [Fact]
    public async Task CommitAsync_CommitsTransactionAndResetsState()
    {
        FakeDbConnection fakeConn = new();
        UnitOfWork uow = new(() => fakeConn);

        await uow.BeginAsync();
        FakeDbTransaction tx = fakeConn.LastTransaction!;

        await uow.CommitAsync();

        Assert.True(tx.WasCommitted);
        Assert.True(tx.WasDisposed);
        Assert.False(uow.HasActiveTransaction);
        Assert.Null(uow.ActiveTransaction);
    }

    [Fact]
    public async Task CommitAsync_ThrowsInvalidOperationException_WhenNoActiveTransaction()
    {
        FakeDbConnection fakeConn = new();
        UnitOfWork uow = new(() => fakeConn);

        await Assert.ThrowsAsync<InvalidOperationException>(() => uow.CommitAsync());
    }

    [Fact]
    public async Task RollbackAsync_RollsBackTransactionAndResetsState()
    {
        FakeDbConnection fakeConn = new();
        UnitOfWork uow = new(() => fakeConn);

        await uow.BeginAsync();
        FakeDbTransaction tx = fakeConn.LastTransaction!;

        await uow.RollbackAsync();

        Assert.True(tx.WasRolledBack);
        Assert.True(tx.WasDisposed);
        Assert.False(uow.HasActiveTransaction);
        Assert.Null(uow.ActiveTransaction);
    }

    [Fact]
    public async Task RollbackAsync_DoesNotThrow_WhenNoActiveTransaction()
    {
        FakeDbConnection fakeConn = new();
        UnitOfWork uow = new(() => fakeConn);

        await uow.RollbackAsync();

        Assert.False(uow.HasActiveTransaction);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ExecutesOperationAndCommits_WhenSuccessful()
    {
        FakeDbConnection fakeConn = new();
        UnitOfWork uow = new(() => fakeConn);
        bool executed = false;

        await uow.ExecuteInTransactionAsync(async ct =>
        {
            executed = true;
            await Task.Yield();
        });

        Assert.True(executed);
        Assert.True(fakeConn.LastTransaction!.WasCommitted);
        Assert.False(uow.HasActiveTransaction);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_RollsBackAndRethrows_WhenOperationFails()
    {
        FakeDbConnection fakeConn = new();
        UnitOfWork uow = new(() => fakeConn);

        await Assert.ThrowsAsync<ApplicationException>(() =>
            uow.ExecuteInTransactionAsync(ct => throw new ApplicationException("Operation failed")));

        Assert.True(fakeConn.LastTransaction!.WasRolledBack);
        Assert.False(uow.HasActiveTransaction);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithResult_ReturnsValueAndCommits()
    {
        FakeDbConnection fakeConn = new();
        UnitOfWork uow = new(() => fakeConn);

        string result = await uow.ExecuteInTransactionAsync(async ct =>
        {
            await Task.Yield();
            return "success";
        });

        Assert.Equal("success", result);
        Assert.True(fakeConn.LastTransaction!.WasCommitted);
        Assert.False(uow.HasActiveTransaction);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithResult_RollsBackAndRethrows_WhenOperationFails()
    {
        FakeDbConnection fakeConn = new();
        UnitOfWork uow = new(() => fakeConn);

        await Assert.ThrowsAsync<ApplicationException>(() =>
            uow.ExecuteInTransactionAsync<string>(ct => throw new ApplicationException("Operation failed")));

        Assert.True(fakeConn.LastTransaction!.WasRolledBack);
        Assert.False(uow.HasActiveTransaction);
    }

    [Fact]
    public async Task DisposeAsync_DisposesTransactionAndConnection()
    {
        FakeDbConnection fakeConn = new();
        UnitOfWork uow = new(() => fakeConn);

        await uow.BeginAsync();
        FakeDbTransaction tx = fakeConn.LastTransaction!;

        await uow.DisposeAsync();

        Assert.True(tx.WasDisposed);
        Assert.True(fakeConn.WasDisposed);
        Assert.False(uow.HasActiveTransaction);
    }
}
