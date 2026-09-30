using GymTron.Domain.Entities;

namespace GymTron.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetById(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameOrEmail(string identifier, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameOrEmail(string username, string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameOrEmail(string username, string email, int excludeUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAll(CancellationToken cancellationToken = default);
    Task<int> Add(User user, CancellationToken cancellationToken = default);
    Task Update(User user, CancellationToken cancellationToken = default);
    Task SoftDelete(int id, CancellationToken cancellationToken = default);
}
