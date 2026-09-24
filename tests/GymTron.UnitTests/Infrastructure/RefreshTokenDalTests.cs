using GymTron.Infrastructure.Persistence.DAL.MySQL;
using MySqlConnector;
using Xunit;

namespace GymTron.UnitTests.Infrastructure;

public class RefreshTokenDalTests
{
    [Fact]
    public async Task DeleteExpiredAndRevoked_WithUnreachableServer_ThrowsMySqlException()
    {
        var dal = new RefreshTokenDAL("Server=127.0.0.1;Port=65534;Database=gymtron;Uid=root;Pwd=wrong;Connection Timeout=1;");

        await Assert.ThrowsAnyAsync<MySqlException>(async () =>
        {
            await dal.DeleteExpiredAndRevoked(DateTime.UtcNow);
        });
    }

    [Fact]
    public async Task DeleteExpiredAndRevoked_WhenCancelled_ThrowsOperationCanceledException()
    {
        var dal = new RefreshTokenDAL("Server=127.0.0.1;Port=65534;Database=gymtron;Uid=root;Pwd=wrong;Connection Timeout=1;");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await dal.DeleteExpiredAndRevoked(DateTime.UtcNow, cts.Token);
        });
    }
}
