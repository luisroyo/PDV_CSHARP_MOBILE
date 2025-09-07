using System.ComponentModel.DataAnnotations;

namespace Pos.Desktop.Wpf.Models
{
    public class CachedProduct
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public bool Active { get; set; }
        public DateTime LastSync { get; set; }
        public bool IsPendingSync { get; set; }
    }
}
