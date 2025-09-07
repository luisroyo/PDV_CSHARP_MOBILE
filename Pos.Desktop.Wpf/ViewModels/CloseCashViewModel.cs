using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;

namespace Pos.Desktop.Wpf.ViewModels
{
    public class CloseCashViewModel : INotifyPropertyChanged
    {
        private DateTime _openDateTime = DateTime.Today.AddHours(8);
        private string _userName = "Operador";
        private decimal _initialBalance = 100.00m;
        private decimal _totalSales = 15420.50m;
        private decimal _totalInflows = 0m;
        private decimal _totalOutflows = 0m;
        private string _statusMessage = "Pronto";
        private CashCountDto _cashCount;
        private ObservableCollection<PaymentMethodDto> _paymentMethods;

        public CloseCashViewModel()
        {
            _cashCount = new CashCountDto();
            _paymentMethods = new ObservableCollection<PaymentMethodDto>();
            
            // Commands
            RefreshCommand = new RelayCommand(Refresh);
            CloseCashCommand = new RelayCommand(CloseCash);
            PrintReportCommand = new RelayCommand(PrintReport);
            SaveReportCommand = new RelayCommand(SaveReport);

            LoadData();
        }

        public DateTime OpenDateTime
        {
            get => _openDateTime;
            set
            {
                _openDateTime = value;
                OnPropertyChanged();
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        public decimal InitialBalance
        {
            get => _initialBalance;
            set
            {
                _initialBalance = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExpectedBalance));
            }
        }

        public decimal TotalSales
        {
            get => _totalSales;
            set
            {
                _totalSales = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExpectedBalance));
            }
        }

        public decimal TotalInflows
        {
            get => _totalInflows;
            set
            {
                _totalInflows = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExpectedBalance));
            }
        }

        public decimal TotalOutflows
        {
            get => _totalOutflows;
            set
            {
                _totalOutflows = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExpectedBalance));
            }
        }

        public decimal ExpectedBalance => InitialBalance + TotalSales + TotalInflows - TotalOutflows;

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public CashCountDto CashCount
        {
            get => _cashCount;
            set
            {
                _cashCount = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PaymentMethodDto> PaymentMethods
        {
            get => _paymentMethods;
            set
            {
                _paymentMethods = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand CloseCashCommand { get; }
        public ICommand PrintReportCommand { get; }
        public ICommand SaveReportCommand { get; }

        private void LoadData()
        {
            // Simular dados de fechamento
            PaymentMethods.Clear();
            
            var methods = new[]
            {
                new PaymentMethodDto { Method = "Dinheiro", Count = 45, Total = 8500.00m },
                new PaymentMethodDto { Method = "Cartão Débito", Count = 32, Total = 4200.50m },
                new PaymentMethodDto { Method = "Cartão Crédito", Count = 28, Total = 2100.00m },
                new PaymentMethodDto { Method = "PIX", Count = 22, Total = 620.00m }
            };

            foreach (var method in methods)
            {
                PaymentMethods.Add(method);
            }

            // Calcular total contado
            CalculateCashTotal();
        }

        private void Refresh()
        {
            LoadData();
            StatusMessage = "Dados atualizados";
        }

        private void CloseCash()
        {
            try
            {
                var result = MessageBox.Show(
                    "Deseja realmente fechar o caixa? Esta ação não pode ser desfeita.",
                    "Confirmar Fechamento",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Simular fechamento
                    StatusMessage = "Caixa fechado com sucesso!";
                    
                    // Mostrar resumo
                    var summary = $"Fechamento realizado em {DateTime.Now:dd/MM/yyyy HH:mm}\n" +
                                 $"Saldo esperado: {ExpectedBalance:C}\n" +
                                 $"Saldo contado: {CashCount.TotalCounted:C}\n" +
                                 $"Diferença: {CashCount.Difference:C}";
                    
                    MessageBox.Show(summary, "Resumo do Fechamento", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao fechar caixa: {ex.Message}";
            }
        }

        private void PrintReport()
        {
            try
            {
                // Simular impressão
                StatusMessage = "Relatório de fechamento enviado para impressão";
                MessageBox.Show("Relatório de fechamento enviado para impressão", "Impressão", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao imprimir: {ex.Message}";
            }
        }

        private void SaveReport()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Arquivos PDF (*.pdf)|*.pdf|Arquivos Excel (*.xlsx)|*.xlsx",
                    FileName = $"Fechamento_Caixa_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    StatusMessage = $"Relatório salvo: {saveDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao salvar relatório: {ex.Message}";
            }
        }

        private void CalculateCashTotal()
        {
            var total = (CashCount.HundredBills * 100) +
                       (CashCount.FiftyBills * 50) +
                       (CashCount.TwentyBills * 20) +
                       (CashCount.TenBills * 10) +
                       (CashCount.FiveBills * 5) +
                       (CashCount.OneCoins * 1) +
                       (CashCount.FiftyCents * 0.5m) +
                       (CashCount.TwentyFiveCents * 0.25m) +
                       (CashCount.TenCents * 0.10m) +
                       (CashCount.FiveCents * 0.05m);

            CashCount.TotalCounted = total;
            CashCount.Difference = total - ExpectedBalance;
            CashCount.DifferenceColor = CashCount.Difference >= 0 ? "#27AE60" : "#E74C3C";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CashCountDto : INotifyPropertyChanged
    {
        private int _hundredBills;
        private int _fiftyBills;
        private int _twentyBills;
        private int _tenBills;
        private int _fiveBills;
        private int _oneCoins;
        private int _fiftyCents;
        private int _twentyFiveCents;
        private int _tenCents;
        private int _fiveCents;
        private decimal _totalCounted;
        private decimal _difference;
        private string _differenceColor = "#2C3E50";

        public int HundredBills
        {
            get => _hundredBills;
            set
            {
                _hundredBills = value;
                OnPropertyChanged();
            }
        }

        public int FiftyBills
        {
            get => _fiftyBills;
            set
            {
                _fiftyBills = value;
                OnPropertyChanged();
            }
        }

        public int TwentyBills
        {
            get => _twentyBills;
            set
            {
                _twentyBills = value;
                OnPropertyChanged();
            }
        }

        public int TenBills
        {
            get => _tenBills;
            set
            {
                _tenBills = value;
                OnPropertyChanged();
            }
        }

        public int FiveBills
        {
            get => _fiveBills;
            set
            {
                _fiveBills = value;
                OnPropertyChanged();
            }
        }

        public int OneCoins
        {
            get => _oneCoins;
            set
            {
                _oneCoins = value;
                OnPropertyChanged();
            }
        }

        public int FiftyCents
        {
            get => _fiftyCents;
            set
            {
                _fiftyCents = value;
                OnPropertyChanged();
            }
        }

        public int TwentyFiveCents
        {
            get => _twentyFiveCents;
            set
            {
                _twentyFiveCents = value;
                OnPropertyChanged();
            }
        }

        public int TenCents
        {
            get => _tenCents;
            set
            {
                _tenCents = value;
                OnPropertyChanged();
            }
        }

        public int FiveCents
        {
            get => _fiveCents;
            set
            {
                _fiveCents = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalCounted
        {
            get => _totalCounted;
            set
            {
                _totalCounted = value;
                OnPropertyChanged();
            }
        }

        public decimal Difference
        {
            get => _difference;
            set
            {
                _difference = value;
                OnPropertyChanged();
            }
        }

        public string DifferenceColor
        {
            get => _differenceColor;
            set
            {
                _differenceColor = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PaymentMethodDto : INotifyPropertyChanged
    {
        private string _method = string.Empty;
        private int _count;
        private decimal _total;

        public string Method
        {
            get => _method;
            set
            {
                _method = value;
                OnPropertyChanged();
            }
        }

        public int Count
        {
            get => _count;
            set
            {
                _count = value;
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
