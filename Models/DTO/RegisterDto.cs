using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace ElectronicsStore.Models.DTO
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "The First Name is required")]
        [MaxLength(100, ErrorMessage = "Max length is 100 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "The Last Name is required")]
        [MaxLength(100, ErrorMessage = "Max length is 100 characters")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "The format of phone is not valid")]
        [MaxLength(20, ErrorMessage = "Max length is 20 characters")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "The Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The Address is required")]
        [MaxLength(200, ErrorMessage = "Max length is 200 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "The Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation do not match")]
        public string ConfirmPassword { get; set; }
    }
}
