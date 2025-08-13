using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace ElectronicsStore.Models.DTO
{
    public class ProductDTO
    {
        [Required(ErrorMessage = "Product name is required.")]
        [MinLength(3, ErrorMessage = "Product name must be at least 3 characters long.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(50, ErrorMessage = "Brand name cannot exceed 50 characters.")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Precision(16, 2)]
        public decimal Price { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
