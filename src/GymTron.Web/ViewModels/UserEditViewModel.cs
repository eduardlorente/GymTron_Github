using System.ComponentModel.DataAnnotations;
using GymTron.Domain.Enums;

namespace GymTron.Web.ViewModels;

public class UserEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Validation_Username_Required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Validation_Username_Length")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_Email_Required")]
    [EmailAddress(ErrorMessage = "Validation_Email_Invalid")]
    [StringLength(255, ErrorMessage = "Validation_Email_Length")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_TypeId_Required")]
    public UserTypes TypeId { get; set; } = UserTypes.Standard;

    public bool IsActive { get; set; } = true;

    [DataType(DataType.Password)]
    [StringLength(128, MinimumLength = 6, ErrorMessage = "Validation_Password_Length")]
    public string? NewPassword { get; set; }
}
