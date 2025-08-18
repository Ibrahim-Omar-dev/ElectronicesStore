using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public ApplicationUser Client { get; set; } // Navigation property to ApplicationUser
        [Precision(18, 2)]
        public double ShippingFee { get; set; }
        public string DeliveryAddress { get; set; }
        public string PaymentMethod { get; set; } // e.g., Credit Card, PayPal, etc.
        public string PaymentStatus { get; set; } // e.g., Paid, Unpaid, Refunded
        public string PaymentDetails { get; set; }
        public DateTime? PaymentDate { get; set; } // Nullable in case payment is not yet made
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public string OrderStatus { get; set; }
    }
}
