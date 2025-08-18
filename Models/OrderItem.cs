using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStore.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        public int Id { get; set; }

        public int Quentity { get; set; }
        [Precision(16, 2)]
        public decimal Price { get; set; }
        public Product Product { get; set; } // Navigation property to Product
    }
}
