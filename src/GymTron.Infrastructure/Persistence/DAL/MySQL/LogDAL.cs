using Dapper;
using GymTron.Domain.Entities;
using MySqlConnector;
using System.Data;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal class LogDAL(string connectionString) : ILogDAL
{


    public async Task Add(Log entity, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            entity.Message,
            entity.Status.CreatedOn
        };

        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string query = @"INSERT INTO logs 
                            (created_on, message) 
                         VALUES 
                            (@CreatedOn, @Message);";

        await dbConnection.ExecuteAsync(new CommandDefinition(query, parameters, cancellationToken: cancellationToken));
    }
}
