using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using Pos.Desktop.Wpf.Models;
using Pos.Desktop.Wpf.Services;
using Pos.Desktop.Wpf.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Pos.Desktop.Wpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly OfflineSyncService _offlineSyncService;
        private readonly PrinterService _printerService;
        private readonly PeripheralService _peripheralService;
        private readonly ILogger<MainViewModel> _logger;
        private ObservableCollection<ProductDto> _products;
        private ObservableCollection<Models.OrderItemDto> _cartItems;
        private ProductDto? _selectedProduct;
        private decimal _cartTotal;
        private string _searchText;
        private string _currentUser;
        private string _businessProfile;
        private bool _isLoading;
        private bool _isOnline;

        public MainViewModel()
        {
            _apiService = new ApiService();
            _offlineSyncService = new OfflineSyncService(
                new LocalDbContext(new DbContextOptionsBuilder<LocalDbContext>()
                    .UseSqlite("Data Source=pos_offline.db")
                    .Options),
                _apiService,
                new Logger<OfflineSyncService>(new LoggerFactory())
            );
            _printerService = new PrinterService(new Logger<PrinterService>(new LoggerFactory()));
            _peripheralService = new PeripheralService(new Logger<PeripheralService>(new LoggerFactory()));
            _logger = new Logger<MainViewModel>(new LoggerFactory());
            
            Products = new ObservableCollection<ProductDto>();
            CartItems = new ObservableCollection<Models.OrderItemDto>();
            SearchCommand = new RelayCommand(SearchProducts);
            AddToCartCommand = new RelayCommand<ProductDto>(AddToCart);
            RemoveFromCartCommand = new RelayCommand<Models.OrderItemDto>(RemoveFromCart);
            ProcessPaymentCommand = new RelayCommand(ProcessPayment, CanProcessPayment);
            PrintReceiptCommand = new RelayCommand(PrintReceipt, CanPrintReceipt);

            _ = InitializeAsync();
            CurrentUser = "Operador";
            BusinessProfile = "Farmácia"; // Será configurado por tenant
            
            // Configurar eventos dos periféricos
            _peripheralService.BarcodeScanned += OnBarcodeScanned;
            _peripheralService.WeightReceived += OnWeightReceived;
        }

        public ObservableCollection<ProductDto> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<Models.OrderItemDto> CartItems
        {
            get => _cartItems;
            set => SetProperty(ref _cartItems, value);
        }

        public ProductDto? SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public decimal CartTotal
        {
            get => _cartTotal;
            set => SetProperty(ref _cartTotal, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string BusinessProfile
        {
            get => _businessProfile;
            set => SetProperty(ref _businessProfile, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsOnline
        {
            get => _isOnline;
            set => SetProperty(ref _isOnline, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand ProcessPaymentCommand { get; }
        public ICommand PrintReceiptCommand { get; }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                
                // Verificar se está online
                IsOnline = await _offlineSyncService.IsOnlineAsync();
                
                if (IsOnline)
                {
                    // Sincronizar produtos online
                    await _offlineSyncService.SyncProductsAsync();
                    await _offlineSyncService.SyncPendingOrdersAsync();
                }
                
                // Carregar produtos (online ou offline)
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao inicializar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var products = await _offlineSyncService.GetCachedProductsAsync(SearchText);
                
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchProducts()
        {
            try
            {
                IsLoading = true;
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddToCart(ProductDto product)
        {
            if (product == null) return;

            var existingItem = CartItems.FirstOrDefault(i => i.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.Qty += 1;
            }
            else
            {
                    var newItem = new Models.OrderItemDto
                {
                    Id = CartItems.Count + 1,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductSku = product.Sku,
                    Qty = 1,
                    UnitPrice = product.Price,
                    Notes = ""
                };
                CartItems.Add(newItem);
            }

            CalculateCartTotal();
        }

        private void RemoveFromCart(Models.OrderItemDto item)
        {
            if (item == null) return;

            CartItems.Remove(item);
            CalculateCartTotal();
        }

        private void CalculateCartTotal()
        {
            CartTotal = CartItems.Sum(i => i.Subtotal);
        }

        private bool CanProcessPayment()
        {
            return CartItems.Any();
        }

        private async void ProcessPayment()
        {
            if (!CanProcessPayment())
            {
                MessageBox.Show("Adicione itens ao carrinho antes de processar o pagamento.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                
                var orderRequest = new CreateOrderRequest
                {
                    CustomerName = "Cliente Avulso",
                    Items = CartItems.Select(item => new CreateOrderItemRequest
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductSku = item.ProductSku,
                        Qty = item.Qty,
                        UnitPrice = item.UnitPrice,
                        Notes = item.Notes
                    }).ToList()
                };

                OrderDto? order = null;

                if (IsOnline)
                {
                    // Tentar criar pedido online
                    order = await _apiService.CreateOrderAsync(orderRequest);
                }
                else
                {
                    // Criar pedido offline
                    order = await _offlineSyncService.CreateOrderOfflineAsync(orderRequest);
                }
                
                if (order != null)
                {
                    var statusMessage = IsOnline ? "criado com sucesso" : "salvo offline (será sincronizado quando online)";
                    MessageBox.Show($"Pedido {order.Number} {statusMessage}!\nTotal: {order.Total:C}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    CartItems.Clear();
                    CalculateCartTotal();
                }
                else
                {
                    MessageBox.Show("Erro ao criar pedido. Tente novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao processar pagamento: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanPrintReceipt()
        {
            return CartItems.Any();
        }

        private async void PrintReceipt()
        {
            if (!CanPrintReceipt())
            {
                MessageBox.Show("Adicione itens ao carrinho antes de imprimir.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                
                // Criar pedido temporário para impressão
                var tempOrder = new OrderDto
                {
                    Id = 0,
                    Number = $"TEMP{DateTime.Now:yyyyMMddHHmmss}",
                    Status = "Draft",
                    Total = CartTotal,
                    CreatedAt = DateTime.UtcNow,
                    CustomerName = "Cliente Avulso",
                    Items = CartItems.ToList()
                };

                var success = await _printerService.PrintReceiptAsync(tempOrder);
                
                if (success)
                {
                    MessageBox.Show("Cupom impresso com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Erro ao imprimir cupom. Verifique a conexão com a impressora.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao imprimir: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OnBarcodeScanned(object? sender, string barcode)
        {
            try
            {
                // Buscar produto pelo código de barras
                var product = Products.FirstOrDefault(p => p.Barcode == barcode);
                if (product != null)
                {
                    AddToCart(product);
                    _logger.LogInformation("Produto adicionado ao carrinho via código de barras: {ProductName}", product.Name);
                }
                else
                {
                    MessageBox.Show($"Produto não encontrado para o código: {barcode}", "Produto não encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar código de barras: {Barcode}", barcode);
            }
        }

        private void OnWeightReceived(object? sender, decimal weight)
        {
            try
            {
                // Atualizar quantidade do último item adicionado com o peso
                if (CartItems.Any())
                {
                    var lastItem = CartItems.Last();
                    lastItem.Qty = (int)weight;
                    CalculateCartTotal();
                    _logger.LogInformation("Peso atualizado para item: {ProductName} - {Weight} kg", lastItem.ProductName, weight);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar peso: {Weight}", weight);
            }
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

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

        public void Execute(object? parameter) => _execute((T?)parameter);
    }
}
