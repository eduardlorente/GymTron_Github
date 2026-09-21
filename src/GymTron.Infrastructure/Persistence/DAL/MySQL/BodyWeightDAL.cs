using Dapper;
using GymTron.Domain.Entities;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL.Extensions;
using MySql.Data.MySqlClient;
using System.Data;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal class BodyWeightDAL(string connectionString) : IBodyWeightDAL
{
    public async Task Add(BodyWeight entity, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            entity.UserId,
            entity.Weight,
            entity.BodyFatPercentage,
            entity.Status.CreatedOn
        };

        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string query = @"INSERT INTO body_weights 
                            (user_id, weight, body_fat_percentage, created_on) 
                         VALUES 
                            (@UserId, @Weight, @BodyFatPercentage, @CreatedOn);";

        await dbConnection.ExecuteAsync(new CommandDefinition(query, parameters, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<BodyWeightDALModel>> ListAll(int? userId = null, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string query = @"SELECT 
                            BW.id AS Id,
                            BW.user_id AS UserId,
                            BW.weight AS Weight,
                            BW.body_fat_percentage AS BodyFatPercentage,
                            BW.created_on AS CreatedOn
                         FROM 
                            body_weights BW
                         WHERE (@UserId IS NULL OR BW.user_id = @UserId);".ToReadUncommited();

        return await dbConnection.QueryAsync<BodyWeightDALModel>(new CommandDefinition(query, new { UserId = userId }, cancellationToken: cancellationToken));
    }
}
