using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;

namespace Pos.Desktop.Wpf.ViewModels
{
    public class PaymentViewModel : INotifyPropertyChanged
    {
        private decimal _totalAmount;
        private string _orderNumber;
        private int _itemCount;
        private string _selectedPaymentMethod = string.Empty;
        private decimal _receivedAmount;
        private decimal _changeAmount;
        private string _cardNumber = string.Empty;
        private string _cardExpiry = string.Empty;
        private string _cardCvv = string.Empty;
        private string _pixKey = string.Empty;
        private string _selectedBank = string.Empty;
        private string _agency = string.Empty;
        private string _account = string.Empty;
        private string _statusMessage = "Selecione a forma de pagamento";

        public PaymentViewModel(decimal totalAmount, string orderNumber, int itemCount)
        {
            _totalAmount = totalAmount;
            _orderNumber = orderNumber;
            _itemCount = itemCount;

            // Initialize banks
            Banks = new ObservableCollection<string>
            {
                "Banco do Brasil", "Caixa Econômica", "Bradesco", "Itaú", "Santander", "Nubank"
            };

            // Commands
            SelectPaymentMethodCommand = new RelayCommand<string>(SelectPaymentMethod);
            ProcessPaymentCommand = new RelayCommand(ProcessPayment, () => CanProcessPayment());
            CancelCommand = new RelayCommand(Cancel);

            // Watch for received amount changes to calculate change
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ReceivedAmount))
                {
                    ChangeAmount = ReceivedAmount - TotalAmount;
                    OnPropertyChanged(nameof(ChangeAmount));
                }
            };
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
            }
        }

        public string OrderNumber
        {
            get => _orderNumber;
            set
            {
                _orderNumber = value;
                OnPropertyChanged();
            }
        }

        public int ItemCount
        {
            get => _itemCount;
            set
            {
                _itemCount = value;
                OnPropertyChanged();
            }
        }

        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCashPayment));
                OnPropertyChanged(nameof(IsCardPayment));
                OnPropertyChanged(nameof(IsPixPayment));
                OnPropertyChanged(nameof(IsTransferPayment));
                ((RelayCommand)ProcessPaymentCommand).RaiseCanExecuteChanged();
            }
        }

        public decimal ReceivedAmount
        {
            get => _receivedAmount;
            set
            {
                _receivedAmount = value;
                OnPropertyChanged();
            }
        }

        public decimal ChangeAmount
        {
            get => _changeAmount;
            set
            {
                _changeAmount = value;
                OnPropertyChanged();
            }
        }

        public string CardNumber
        {
            get => _cardNumber;
            set
            {
                _cardNumber = value;
                OnPropertyChanged();
                ((RelayCommand)ProcessPaymentCommand).RaiseCanExecuteChanged();
            }
        }

        public string CardExpiry
        {
            get => _cardExpiry;
            set
            {
                _cardExpiry = value;
                OnPropertyChanged();
                ((RelayCommand)ProcessPaymentCommand).RaiseCanExecuteChanged();
            }
        }

        public string CardCvv
        {
            get => _cardCvv;
            set
            {
                _cardCvv = value;
                OnPropertyChanged();
                ((RelayCommand)ProcessPaymentCommand).RaiseCanExecuteChanged();
            }
        }

        public string PixKey
        {
            get => _pixKey;
            set
            {
                _pixKey = value;
                OnPropertyChanged();
                ((RelayCommand)ProcessPaymentCommand).RaiseCanExecuteChanged();
            }
        }

        public string SelectedBank
        {
            get => _selectedBank;
            set
            {
                _selectedBank = value;
                OnPropertyChanged();
                ((RelayCommand)ProcessPaymentCommand).RaiseCanExecuteChanged();
            }
        }

        public string Agency
        {
            get => _agency;
            set
            {
                _agency = value;
                OnPropertyChanged();
                ((RelayCommand)ProcessPaymentCommand).RaiseCanExecuteChanged();
            }
        }

        public string Account
        {
            get => _account;
            set
            {
                _account = value;
                OnPropertyChanged();
                ((RelayCommand)ProcessPaymentCommand).RaiseCanExecuteChanged();
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

        public bool IsCashPayment => SelectedPaymentMethod == "Cash";
        public bool IsCardPayment => SelectedPaymentMethod == "Card";
        public bool IsPixPayment => SelectedPaymentMethod == "Pix";
        public bool IsTransferPayment => SelectedPaymentMethod == "Transfer";

        public ObservableCollection<string> Banks { get; }

        public ICommand SelectPaymentMethodCommand { get; }
        public ICommand ProcessPaymentCommand { get; }
        public ICommand CancelCommand { get; }

        private void SelectPaymentMethod(string method)
        {
            SelectedPaymentMethod = method;
            StatusMessage = $"Forma de pagamento selecionada: {GetPaymentMethodName(method)}";
        }

        private string GetPaymentMethodName(string method)
        {
            return method switch
            {
                "Cash" => "Dinheiro",
                "Card" => "Cartão",
                "Pix" => "PIX",
                "Transfer" => "Transferência",
                _ => method
            };
        }

        private bool CanProcessPayment()
        {
            return SelectedPaymentMethod switch
            {
                "Cash" => ReceivedAmount >= TotalAmount,
                "Card" => !string.IsNullOrWhiteSpace(CardNumber) && 
                         !string.IsNullOrWhiteSpace(CardExpiry) && 
                         !string.IsNullOrWhiteSpace(CardCvv),
                "Pix" => !string.IsNullOrWhiteSpace(PixKey),
                "Transfer" => !string.IsNullOrWhiteSpace(SelectedBank) && 
                             !string.IsNullOrWhiteSpace(Agency) && 
                             !string.IsNullOrWhiteSpace(Account),
                _ => false
            };
        }

        private void ProcessPayment()
        {
            try
            {
                StatusMessage = "Processando pagamento...";

                // Simular processamento
                System.Threading.Thread.Sleep(2000);

                // Simular resultado do pagamento
                var success = true; // Em produção, seria baseado na resposta do gateway

                if (success)
                {
                    StatusMessage = "Pagamento processado com sucesso!";
                    
                    var result = MessageBox.Show(
                        "Pagamento processado com sucesso!\n\nDeseja imprimir o cupom?",
                        "Pagamento Aprovado",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Simular impressão
                        StatusMessage = "Cupom enviado para impressão";
                    }

                    // Fechar janela
                    CloseWindow();
                }
                else
                {
                    StatusMessage = "Erro no processamento do pagamento";
                    MessageBox.Show("Erro no processamento do pagamento. Tente novamente.", "Erro de Pagamento", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro: {ex.Message}";
                MessageBox.Show($"Erro ao processar pagamento: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            var result = MessageBox.Show(
                "Deseja realmente cancelar o pagamento?",
                "Cancelar Pagamento",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CloseWindow();
            }
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is PaymentWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
