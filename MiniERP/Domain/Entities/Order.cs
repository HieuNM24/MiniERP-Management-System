using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty; // VD: ORD-20260727-001
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, CANCELLED
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User CreatedByUser { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
