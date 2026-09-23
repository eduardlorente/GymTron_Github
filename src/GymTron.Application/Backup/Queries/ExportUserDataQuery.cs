using GymTron.Application.Backup.DTOs;
using GymTron.Application.Base;

namespace GymTron.Application.Backup.Queries;

public class ExportUserDataQuery(Guid correlationId, int? userId = null) : QueryBase<UserDataBackupDto>(correlationId)
{
    public int? UserId { get; } = userId;
}
