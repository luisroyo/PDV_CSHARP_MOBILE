using System.ComponentModel.DataAnnotations;

namespace Pos.Desktop.Wpf.Models
{
    public class SyncQueue
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty; // "Product", "Order", etc.
        public string Operation { get; set; } = string.Empty; // "Create", "Update", "Delete"
        public int EntityId { get; set; }
        public string EntityData { get; set; } = string.Empty; // JSON serialized data
        public DateTime CreatedAt { get; set; }
        public int RetryCount { get; set; }
        public DateTime? LastRetryAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
