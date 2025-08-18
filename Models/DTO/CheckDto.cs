using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.Models.DTO
{
    public class CheckDto
    {
        [Required]
        [Display(Name ="Delviery Address")]
        public string Delviery { get; set; }
        [Required]
        public string PaymentMethod { get; set; }
    }
}
