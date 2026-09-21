using Dapper;
using MySql.Data.MySqlClient;
using Testcontainers.MySql;

namespace GymTron.IntegrationTests.Database;

[CollectionDefinition(Name)]
public sealed class MySqlCollection : ICollectionFixture<MySqlCollectionFixture>
{
    public const string Name = "MySQL DAL integration";
}

public sealed class MySqlCollectionFixture : IAsyncLifetime
{
    private readonly MySqlContainer container;

    public MySqlCollectionFixture()
    {
        string suffix = Guid.NewGuid().ToString("N");
        container = new MySqlBuilder("mysql:8.0")
            .WithDatabase($"gymtron_{suffix}")
            .WithUsername($"user_{suffix[..12]}")
            .WithPassword($"pw_{Guid.NewGuid():N}")
            .Build();
    }

    public string ConnectionString => container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await container.StartAsync();
        string schema = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Database", "schema.sql"));
        await ExecuteAsync(schema);
    }

    public Task DisposeAsync() => container.DisposeAsync().AsTask();

    public async Task ResetAsync()
    {
        const string sql = """
            SET FOREIGN_KEY_CHECKS = 0;
            TRUNCATE TABLE exercises;
            TRUNCATE TABLE trainings;
            TRUNCATE TABLE routine_items;
            TRUNCATE TABLE exercise_parameters;
            TRUNCATE TABLE routines;
            TRUNCATE TABLE body_weights;
            TRUNCATE TABLE logs;
            SET FOREIGN_KEY_CHECKS = 1;
            """;
        await ExecuteAsync(sql);
    }

    public async Task ExecuteAsync(string sql, object? parameters = null)
    {
        await using MySqlConnection connection = new(ConnectionString);
        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<T> QuerySingleAsync<T>(string sql, object? parameters = null)
    {
        await using MySqlConnection connection = new(ConnectionString);
        return await connection.QuerySingleAsync<T>(sql, parameters);
    }

    public async Task<int> SeedRoutineAsync(string name = "Routine")
    {
        const string sql = "INSERT INTO routines (name) VALUES (@Name); SELECT LAST_INSERT_ID();";
        return await QuerySingleAsync<int>(sql, new { Name = name });
    }

    public async Task<int> SeedParameterAsync(string name = "Squat", int typeId = 1)
    {
        const string sql = """
            INSERT INTO exercise_parameters (name, description, pattern, type_id, replays_in_reserve)
            VALUES (@Name, 'description', 'pattern', @TypeId, 2);
            SELECT LAST_INSERT_ID();
            """;
        return await QuerySingleAsync<int>(sql, new { Name = name, TypeId = typeId });
    }

    public async Task<int> SeedTrainingAsync(int routineId, DateTime? startedOn = null, int status = 1, DateTime? completedOn = null)
    {
        const string sql = """
            INSERT INTO trainings (routine_id, day_of_week, started_on, completed_on, status)
            VALUES (@RoutineId, 2, @StartedOn, @CompletedOn, @Status);
            SELECT LAST_INSERT_ID();
            """;
        return await QuerySingleAsync<int>(sql, new
        {
            RoutineId = routineId,
            StartedOn = startedOn ?? new DateTime(2026, 1, 2, 3, 4, 5),
            CompletedOn = completedOn,
            Status = status
        });
    }
}

public abstract class MySqlIntegrationTest(MySqlCollectionFixture fixture) : IAsyncLifetime
{
    protected MySqlCollectionFixture Database { get; } = fixture;
    protected string ConnectionString => Database.ConnectionString;

    public Task InitializeAsync() => Database.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;
}
