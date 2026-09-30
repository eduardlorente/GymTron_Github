using GymTron.Domain.Enums;

namespace GymTron.Application.Users.DTO;

public class UserDto
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserTypes TypeId { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
