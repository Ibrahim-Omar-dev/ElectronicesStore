using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.Models.DTO
{
    public class EditProfile
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

    }
}
