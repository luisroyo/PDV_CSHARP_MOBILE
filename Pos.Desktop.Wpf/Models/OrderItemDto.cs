using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pos.Desktop.Wpf.Models
{
    public class OrderItemDto : INotifyPropertyChanged
    {
        private int _id;
        private int _productId;
        private string _productName = string.Empty;
        private string _productSku = string.Empty;
        private int _qty;
        private decimal _unitPrice;
        private decimal _subtotal;
        private string _notes = string.Empty;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public int ProductId
        {
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        public string ProductName
        {
            get => _productName;
            set => SetProperty(ref _productName, value);
        }

        public string ProductSku
        {
            get => _productSku;
            set => SetProperty(ref _productSku, value);
        }

        public int Qty
        {
            get => _qty;
            set
            {
                if (SetProperty(ref _qty, value))
                {
                    Subtotal = Qty * UnitPrice;
                }
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (SetProperty(ref _unitPrice, value))
                {
                    Subtotal = Qty * UnitPrice;
                }
            }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set => SetProperty(ref _subtotal, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
