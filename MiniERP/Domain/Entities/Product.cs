using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = string.Empty; // Mã quản lý kho (Mã vạch)
        public string ProductName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; } = 10; // Cảnh báo khi tồn kho dưới mức này
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public Category Category { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
