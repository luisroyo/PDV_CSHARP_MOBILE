using System.ComponentModel.DataAnnotations;

namespace Pos.Mobile.Maui.Models
{
    public class CachedProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Barcode { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastSync { get; set; }
        public string JsonData { get; set; } = string.Empty;
    }

    public class CachedOrder
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CustomerName { get; set; }
        public DateTime LastSync { get; set; }
        public string JsonData { get; set; } = string.Empty;
    }

    public class CachedDashboard
    {
        public int Id { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public DateTime LastSync { get; set; }
        public string JsonData { get; set; } = string.Empty;
    }

    public class SyncQueue
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public string JsonData { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int RetryCount { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
