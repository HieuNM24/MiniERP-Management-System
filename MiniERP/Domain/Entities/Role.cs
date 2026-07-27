using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty; // Admin, InventoryManager, Sales
        public string? Description { get; set; }

        // Navigation Property (Liên kết 1-N với User)
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}

