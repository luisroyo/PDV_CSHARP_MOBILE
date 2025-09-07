using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;

namespace Pos.Desktop.Wpf.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private DateTime _startDate = DateTime.Today.AddDays(-30);
        private DateTime _endDate = DateTime.Today;
        private decimal _totalSales;
        private int _totalOrders;
        private decimal _averageTicket;
        private string _topProduct = string.Empty;
        private string _statusMessage = "Pronto";
        private ObservableCollection<CategorySalesDto> _salesByCategory;
        private ObservableCollection<TopProductDto> _topProducts;

        public ReportsViewModel()
        {
            _salesByCategory = new ObservableCollection<CategorySalesDto>();
            _topProducts = new ObservableCollection<TopProductDto>();
            
            // Commands
            RefreshCommand = new RelayCommand(Refresh);
            GenerateReportsCommand = new RelayCommand(GenerateReports);
            ExportSalesReportCommand = new RelayCommand(ExportSalesReport);
            ExportProductsReportCommand = new RelayCommand(ExportProductsReport);
            ExportCustomersReportCommand = new RelayCommand(ExportCustomersReport);
            ExportFinancialReportCommand = new RelayCommand(ExportFinancialReport);

            GenerateReports();
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

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

        public decimal AverageTicket
        {
            get => _averageTicket;
            set
            {
                _averageTicket = value;
                OnPropertyChanged();
            }
        }

        public string TopProduct
        {
            get => _topProduct;
            set
            {
                _topProduct = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CategorySalesDto> SalesByCategory
        {
            get => _salesByCategory;
            set
            {
                _salesByCategory = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TopProductDto> TopProducts
        {
            get => _topProducts;
            set
            {
                _topProducts = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand GenerateReportsCommand { get; }
        public ICommand ExportSalesReportCommand { get; }
        public ICommand ExportProductsReportCommand { get; }
        public ICommand ExportCustomersReportCommand { get; }
        public ICommand ExportFinancialReportCommand { get; }

        private void Refresh()
        {
            GenerateReports();
        }

        private void GenerateReports()
        {
            try
            {
                StatusMessage = "Gerando relatórios...";

                // Simular dados de relatórios
                TotalSales = 15420.50m;
                TotalOrders = 127;
                AverageTicket = TotalOrders > 0 ? TotalSales / TotalOrders : 0;
                TopProduct = "Paracetamol 500mg";

                // Vendas por categoria
                SalesByCategory.Clear();
                var categorySales = new[]
                {
                    new CategorySalesDto { Category = "Medicamentos", Sales = 8500.00m, Percentage = 55.1 },
                    new CategorySalesDto { Category = "Alimentos", Sales = 3200.50m, Percentage = 20.8 },
                    new CategorySalesDto { Category = "Construção", Sales = 2100.00m, Percentage = 13.6 },
                    new CategorySalesDto { Category = "Bebidas", Sales = 1620.00m, Percentage = 10.5 }
                };

                foreach (var item in categorySales)
                {
                    SalesByCategory.Add(item);
                }

                // Top produtos
                TopProducts.Clear();
                var topProducts = new[]
                {
                    new TopProductDto { Name = "Paracetamol 500mg", Quantity = 45, Total = 562.50m },
                    new TopProductDto { Name = "Arroz 5kg", Quantity = 12, Total = 226.80m },
                    new TopProductDto { Name = "Cimento 50kg", Quantity = 8, Total = 200.00m },
                    new TopProductDto { Name = "Hambúrguer", Quantity = 25, Total = 375.00m },
                    new TopProductDto { Name = "Refrigerante 350ml", Quantity = 30, Total = 135.00m }
                };

                foreach (var item in topProducts)
                {
                    TopProducts.Add(item);
                }

                StatusMessage = $"Relatórios gerados - Período: {StartDate:dd/MM/yyyy} a {EndDate:dd/MM/yyyy}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao gerar relatórios: {ex.Message}";
            }
        }

        private void ExportSalesReport()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Arquivos Excel (*.xlsx)|*.xlsx|Arquivos CSV (*.csv)|*.csv",
                    FileName = $"Relatorio_Vendas_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Simular exportação
                    StatusMessage = $"Relatório de vendas exportado: {saveDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao exportar relatório: {ex.Message}";
            }
        }

        private void ExportProductsReport()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Arquivos Excel (*.xlsx)|*.xlsx|Arquivos CSV (*.csv)|*.csv",
                    FileName = $"Relatorio_Produtos_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    StatusMessage = $"Relatório de produtos exportado: {saveDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao exportar relatório: {ex.Message}";
            }
        }

        private void ExportCustomersReport()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Arquivos Excel (*.xlsx)|*.xlsx|Arquivos CSV (*.csv)|*.csv",
                    FileName = $"Relatorio_Clientes_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    StatusMessage = $"Relatório de clientes exportado: {saveDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao exportar relatório: {ex.Message}";
            }
        }

        private void ExportFinancialReport()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Arquivos Excel (*.xlsx)|*.xlsx|Arquivos CSV (*.csv)|*.csv",
                    FileName = $"Relatorio_Financeiro_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    StatusMessage = $"Relatório financeiro exportado: {saveDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao exportar relatório: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CategorySalesDto : INotifyPropertyChanged
    {
        private string _category = string.Empty;
        private decimal _sales;
        private double _percentage;

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        public decimal Sales
        {
            get => _sales;
            set
            {
                _sales = value;
                OnPropertyChanged();
            }
        }

        public double Percentage
        {
            get => _percentage;
            set
            {
                _percentage = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TopProductDto : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private int _quantity;
        private decimal _total;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
