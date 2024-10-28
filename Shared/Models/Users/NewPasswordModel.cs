using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Users;

public class NewPasswordModel
{
    [Required(ErrorMessage = "Old password is required")]
    public string? OldPassword { get; set; }
    public string? HashedPassword { get; set; }
    [Required(ErrorMessage = "Password is required")]
    [StringLength(255, ErrorMessage = "Must be between 8 and 255 characters", MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm Password is required")]
    [StringLength(255, ErrorMessage = "Must be between 8 and 255 characters", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Compare("NewPassword")]
    public string? ConfirmPassword { get; set; }

}
