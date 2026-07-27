using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = string.Empty; // CREATE_ORDER, UPDATE_PRODUCT, LOGIN
        public string? TableName { get; set; }
        public int? RecordId { get; set; }
        public string? OldValues { get; set; } // JSON chứa dữ liệu cũ
        public string? NewValues { get; set; } // JSON chứa dữ liệu mới
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
