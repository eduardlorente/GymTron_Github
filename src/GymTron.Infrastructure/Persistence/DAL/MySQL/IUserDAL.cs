using GymTron.Infrastructure.Persistence.DAL.Models;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal interface IUserDAL
{
    Task<UserDALModel?> GetById(int id, CancellationToken cancellationToken = default);
    Task<UserDALModel?> GetByUsernameOrEmail(string identifier, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameOrEmail(string username, string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameOrEmail(string username, string email, int excludeUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDALModel>> GetAll(CancellationToken cancellationToken = default);
    Task<int> Add(string username, string email, string passwordHash, int typeId, DateTime createdAt, CancellationToken cancellationToken = default);
    Task Update(int id, string username, string email, string passwordHash, int typeId, bool isActive, CancellationToken cancellationToken = default);
    Task SoftDelete(int id, CancellationToken cancellationToken = default);
}
