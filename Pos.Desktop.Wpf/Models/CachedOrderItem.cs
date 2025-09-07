using System.ComponentModel.DataAnnotations;

namespace Pos.Desktop.Wpf.Models
{
    public class CachedOrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime LastSync { get; set; }

        public CachedOrder Order { get; set; } = null!;
    }
}
