using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pos.Mobile.Maui.Models
{
    public class OrderDto : INotifyPropertyChanged
    {
        private int _id;
        private string _number = string.Empty;
        private string _status = string.Empty;
        private decimal _total;
        private DateTime _createdAt;
        private string? _customerName;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public string? CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
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
