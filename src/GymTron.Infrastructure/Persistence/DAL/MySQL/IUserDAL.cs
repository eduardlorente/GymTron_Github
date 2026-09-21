using GymTron.Infrastructure.Persistence.DAL.Models;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal interface IUserDAL
{
    Task<UserDALModel?> GetById(int id, CancellationToken cancellationToken = default);
    Task<UserDALModel?> GetByUsernameOrEmail(string identifier, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameOrEmail(string username, string email, CancellationToken cancellationToken = default);
    Task<int> Add(string username, string email, string passwordHash, DateTime createdAt, CancellationToken cancellationToken = default);
}
