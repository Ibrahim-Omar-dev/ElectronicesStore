using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.Models.DTO
{
    public class LoginDto
    {
        [Required(ErrorMessage = "The Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        [Required(ErrorMessage = "The Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }
        [Display(Name = "Remember Me")]
        public bool Remberme { get; set; }
    }
}
