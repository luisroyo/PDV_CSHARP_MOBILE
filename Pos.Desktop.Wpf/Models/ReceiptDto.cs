using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pos.Desktop.Wpf.Models
{
    public class ReceiptDto : INotifyPropertyChanged
    {
        private string _orderNumber = string.Empty;
        private List<OrderItemDto> _items = new();
        private decimal _subtotal;
        private decimal _tax;
        private decimal _total;
        private string _paymentMethod = string.Empty;
        private string _cashier = string.Empty;
        private DateTime _dateTime;

        public string OrderNumber
        {
            get => _orderNumber;
            set
            {
                _orderNumber = value;
                OnPropertyChanged();
            }
        }

        public List<OrderItemDto> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set
            {
                _subtotal = value;
                OnPropertyChanged();
            }
        }

        public decimal Tax
        {
            get => _tax;
            set
            {
                _tax = value;
                OnPropertyChanged();
            }
        }

        public decimal Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged();
            }
        }

        public string PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                _paymentMethod = value;
                OnPropertyChanged();
            }
        }

        public string Cashier
        {
            get => _cashier;
            set
            {
                _cashier = value;
                OnPropertyChanged();
            }
        }

        public DateTime DateTime
        {
            get => _dateTime;
            set
            {
                _dateTime = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
