using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.Models.DTO
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "The Password is required")]
        [DataType(DataType.Password), Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "The Password is required")]
        [DataType(DataType.Password), Display(Name = "New Password")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password), Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Password and confirmation do not match")]
        public string ConfirmPassword { get; set; }
    }
}
