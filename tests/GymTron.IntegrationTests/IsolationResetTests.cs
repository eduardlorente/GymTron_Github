using Dapper;
using GymTron.Infrastructure.Persistence.DAL.MySQL.Extensions;
using GymTron.IntegrationTests.Database;
using MySql.Data.MySqlClient;

namespace GymTron.IntegrationTests;

[Collection(MySqlCollection.Name)]
public sealed class IsolationResetTests(MySqlCollectionFixture fixture) : MySqlIntegrationTest(fixture)
{
    [Fact]
    public async Task ToReadUncommited_WhenQueryFails_ResetsIsolationLevel()
    {
        await using MySqlConnection connection = new(ConnectionString);
        await connection.OpenAsync();

        string failingQuery = @"SELECT * FROM nonexistent_table;".ToReadUncommited();

        await Assert.ThrowsAnyAsync<Exception>(() => connection.ExecuteAsync(failingQuery));

        string isolationLevel = await connection.QuerySingleAsync<string>(
            "SELECT @@session.transaction_isolation");

        Assert.Equal("REPEATABLE-READ", isolationLevel);
    }

    [Fact]
    public async Task ToReadUncommited_WhenQuerySucceeds_ResetsIsolationLevel()
    {
        await using MySqlConnection connection = new(ConnectionString);
        await connection.OpenAsync();

        string successfulQuery = @"SELECT 1;".ToReadUncommited();
        await connection.ExecuteAsync(successfulQuery);

        string isolationLevel = await connection.QuerySingleAsync<string>(
            "SELECT @@session.transaction_isolation");

        Assert.Equal("REPEATABLE-READ", isolationLevel);
    }
}
