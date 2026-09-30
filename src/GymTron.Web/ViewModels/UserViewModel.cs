using GymTron.Domain.Enums;

namespace GymTron.Web.ViewModels;

public class UserViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserTypes TypeId { get; set; }
    public string TypeName => TypeId.ToString();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
