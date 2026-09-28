using System.ComponentModel.DataAnnotations;
using GymTron.Domain.Enums;

namespace GymTron.Web.ViewModels;

public class UserEditViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public UserTypes TypeId { get; set; } = UserTypes.Standard;

    public bool IsActive { get; set; } = true;

    [DataType(DataType.Password)]
    [StringLength(128, MinimumLength = 10)]
    public string? NewPassword { get; set; }
}
