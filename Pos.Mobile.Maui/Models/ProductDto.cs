using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pos.Mobile.Maui.Models
{
    public class ProductDto : INotifyPropertyChanged
    {
        private int _id;
        private string _sku = string.Empty;
        private string _name = string.Empty;
        private string? _description;
        private decimal _price;
        private string? _barcode;
        private string? _category;
        private bool _active;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Sku
        {
            get => _sku;
            set => SetProperty(ref _sku, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public string? Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string? Barcode
        {
            get => _barcode;
            set => SetProperty(ref _barcode, value);
        }

        public string? Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public bool Active
        {
            get => _active;
            set => SetProperty(ref _active, value);
        }

        public bool IsActive
        {
            get => _active;
            set => SetProperty(ref _active, value);
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
