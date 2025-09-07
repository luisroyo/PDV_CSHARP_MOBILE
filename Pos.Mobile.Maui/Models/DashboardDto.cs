using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pos.Mobile.Maui.Models
{
    public class DashboardDto : INotifyPropertyChanged
    {
        private decimal _totalSales;
        private int _totalOrders;
        private int _totalProducts;
        private decimal _averageTicket;
        private int _todayOrders;
        private decimal _todaySales;

        public decimal TotalSales
        {
            get => _totalSales;
            set
            {
                _totalSales = value;
                OnPropertyChanged();
            }
        }

        public int TotalOrders
        {
            get => _totalOrders;
            set
            {
                _totalOrders = value;
                OnPropertyChanged();
            }
        }

        public int TotalProducts
        {
            get => _totalProducts;
            set
            {
                _totalProducts = value;
                OnPropertyChanged();
            }
        }

        public decimal AverageTicket
        {
            get => _averageTicket;
            set
            {
                _averageTicket = value;
                OnPropertyChanged();
            }
        }

        public int TodayOrders
        {
            get => _todayOrders;
            set
            {
                _todayOrders = value;
                OnPropertyChanged();
            }
        }

        public decimal TodaySales
        {
            get => _todaySales;
            set
            {
                _todaySales = value;
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
