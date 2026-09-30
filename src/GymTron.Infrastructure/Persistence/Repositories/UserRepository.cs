using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Repositories;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;

namespace GymTron.Infrastructure.Persistence.Repositories;

internal class UserRepository(IUserDAL userDAL) : IUserRepository
{
    private readonly IUserDAL _userDAL = userDAL;

    public async Task<User?> GetById(int id, CancellationToken cancellationToken = default)
    {
        UserDALModel? model = await _userDAL.GetById(id, cancellationToken);
        if (model == null)
        {
            return null;
        }

        return User.FromDatabase(
            model.Id,
            model.Username,
            model.Email,
            model.PasswordHash,
            (UserTypes)model.TypeId,
            model.IsActive,
            model.CreatedAt);
    }

    public async Task<User?> GetByUsernameOrEmail(string identifier, CancellationToken cancellationToken = default)
    {
        UserDALModel? model = await _userDAL.GetByUsernameOrEmail(identifier, cancellationToken);
        if (model == null)
        {
            return null;
        }

        return User.FromDatabase(
            model.Id,
            model.Username,
            model.Email,
            model.PasswordHash,
            (UserTypes)model.TypeId,
            model.IsActive,
            model.CreatedAt);
    }

    public async Task<bool> ExistsByUsernameOrEmail(string username, string email, CancellationToken cancellationToken = default)
    {
        return await _userDAL.ExistsByUsernameOrEmail(username, email, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameOrEmail(string username, string email, int excludeUserId, CancellationToken cancellationToken = default)
    {
        return await _userDAL.ExistsByUsernameOrEmail(username, email, excludeUserId, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAll(CancellationToken cancellationToken = default)
    {
        var models = await _userDAL.GetAll(cancellationToken);
        return models.Select(model => User.FromDatabase(
            model.Id,
            model.Username,
            model.Email,
            model.PasswordHash,
            (UserTypes)model.TypeId,
            model.IsActive,
            model.CreatedAt)).ToList();
    }

    public async Task<int> Add(User user, CancellationToken cancellationToken = default)
    {
        return await _userDAL.Add(
            user.Username,
            user.Email,
            user.PasswordHash,
            (int)user.TypeId,
            user.Status.CreatedOn,
            cancellationToken);
    }

    public async Task Update(User user, CancellationToken cancellationToken = default)
    {
        await _userDAL.Update(
            user.Id,
            user.Username,
            user.Email,
            user.PasswordHash,
            (int)user.TypeId,
            user.Status.IsActive,
            cancellationToken);
    }

    public async Task SoftDelete(int id, CancellationToken cancellationToken = default)
    {
        await _userDAL.SoftDelete(id, cancellationToken);
    }
}
