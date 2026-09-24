using System.Data;
using System.Data.Common;
using GymTron.Domain.Common;
using MySqlConnector;

namespace GymTron.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork, IDbTransactionContext, IDisposable
{
    private readonly string? _connectionString;
    private readonly Func<DbConnection>? _connectionFactory;
    private DbConnection? _connection;
    private DbTransaction? _transaction;

    public UnitOfWork(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    internal UnitOfWork(Func<DbConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IDbConnection? ActiveConnection => _connection;

    public IDbTransaction? ActiveTransaction => _transaction;

    public bool HasActiveTransaction => _transaction != null && _connection != null && _connection.State == ConnectionState.Open;

    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        if (_connection == null)
        {
            _connection = _connectionFactory != null
                ? _connectionFactory()
                : new MySqlConnection(_connectionString);
            await _connection.OpenAsync(cancellationToken);
        }
        else if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }

        _transaction = await _connection.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await BeginAsync(cancellationToken);
        try
        {
            await operation(cancellationToken);
            await CommitAsync(cancellationToken);
        }
        catch
        {
            try
            {
                await RollbackAsync(cancellationToken);
            }
            catch
            {
                // Suppress rollback failure to preserve original exception
            }

            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await BeginAsync(cancellationToken);
        try
        {
            T result = await operation(cancellationToken);
            await CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            try
            {
                await RollbackAsync(cancellationToken);
            }
            catch
            {
                // Suppress rollback failure to preserve original exception
            }

            throw;
        }
    }

    public void Dispose()
    {
        DisposeTransaction();

        if (_connection != null)
        {
            _connection.Dispose();
            _connection = null;
        }

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();

        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }

        GC.SuppressFinalize(this);
    }

    private void DisposeTransaction()
    {
        if (_transaction != null)
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
