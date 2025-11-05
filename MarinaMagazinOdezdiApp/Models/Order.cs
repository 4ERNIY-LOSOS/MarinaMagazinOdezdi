using System;
using System.Collections.Generic;

namespace MarinaMagazinOdezdiApp.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        // Shipping Address
        public string ShippingCity { get; set; }
        public string ShippingStreet { get; set; }
        public string ShippingHouseNumber { get; set; }

        // Navigation properties
        public User User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}