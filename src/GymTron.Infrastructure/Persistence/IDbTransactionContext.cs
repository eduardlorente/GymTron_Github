using System.Data;

namespace GymTron.Infrastructure.Persistence;

internal interface IDbTransactionContext
{
    IDbConnection? ActiveConnection { get; }
    IDbTransaction? ActiveTransaction { get; }
    bool HasActiveTransaction { get; }
}
