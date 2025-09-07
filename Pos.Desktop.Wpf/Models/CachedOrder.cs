using System.ComponentModel.DataAnnotations;

namespace Pos.Desktop.Wpf.Models
{
    public class CachedOrder
    {
        public int Id { get; set; }
        public int? ServerId { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime LastSync { get; set; }
        public bool IsPendingSync { get; set; }

        public List<CachedOrderItem> Items { get; set; } = new();
    }
}
