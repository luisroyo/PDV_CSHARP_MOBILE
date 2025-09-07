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
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Models.OrderItemDto> CartItems
        {
            get => _cartItems;
            set
            {
                _cartItems = value;
                OnPropertyChanged();
                CalculateCartTotal();
            }
        }

        public ProductDto? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
            }
        }

        public decimal CartTotal
        {
            get => _cartTotal;
            set
            {
                _cartTotal = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public string CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        public string BusinessProfile
        {
            get => _businessProfile;
            set
            {
                _businessProfile = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool IsOnline
        {
            get => _isOnline;
            set
            {
                _isOnline = value;
                OnPropertyChanged();
            }
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
                
                // Carregar produtos
                await LoadProductsAsync();
                
                // Sincronizar dados offline se online
                if (IsOnline)
                {
                    await _offlineSyncService.SyncOfflineDataAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar aplicação");
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
                var products = await _offlineSyncService.GetProductsAsync();
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar produtos");
            }
        }

        private void SearchProducts()
        {
            // Implementar busca de produtos
            _logger.LogInformation("Buscando produtos com termo: {SearchText}", SearchText);
        }

        private void AddToCart(ProductDto product)
        {
            if (product == null) return;

            var existingItem = CartItems.FirstOrDefault(i => i.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.Qty++;
            }
            else
            {
                CartItems.Add(new Models.OrderItemDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductSku = product.Sku,
                    Qty = 1,
                    UnitPrice = product.Price,
                    Notes = string.Empty
                });
            }

            CalculateCartTotal();
            _logger.LogInformation("Produto adicionado ao carrinho: {ProductName}", product.Name);
        }

        private void RemoveFromCart(Models.OrderItemDto item)
        {
            if (item == null) return;

            CartItems.Remove(item);
            CalculateCartTotal();
            _logger.LogInformation("Item removido do carrinho: {ProductName}", item.ProductName);
        }

        private void CalculateCartTotal()
        {
            CartTotal = CartItems.Sum(i => i.Subtotal);
        }

        private bool CanProcessPayment()
        {
            return CartItems.Any();
        }

        private void ProcessPayment()
        {
            if (!CanProcessPayment())
            {
                MessageBox.Show("Adicione itens ao carrinho antes de processar o pagamento.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _logger.LogInformation("Processando pagamento para {ItemCount} itens", CartItems.Count);

                // Abrir janela de pagamento
                var orderNumber = $"ORD-{DateTime.Now:yyyyMMdd-HHmmss}";
                var paymentWindow = new Views.PaymentWindow(CartTotal, orderNumber, CartItems.Count);
                paymentWindow.ShowDialog();

                // Se o pagamento foi processado com sucesso, limpar o carrinho
                // (Em produção, isso seria controlado pelo resultado da janela de pagamento)
                CartItems.Clear();
                CalculateCartTotal();
                
                _logger.LogInformation("Pagamento processado. Pedido: {OrderNumber}", orderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pagamento");
                MessageBox.Show($"Erro ao processar pagamento: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
                _logger.LogInformation("Imprimindo cupom para {ItemCount} itens", CartItems.Count);
                
                var receipt = new ReceiptDto
                {
                    OrderNumber = $"ORD-{DateTime.Now:yyyyMMdd-HHmmss}",
                    Items = CartItems.ToList(),
                    Subtotal = CartTotal,
                    Tax = 0, // Implementar cálculo de impostos
                    Total = CartTotal,
                    PaymentMethod = "Dinheiro", // Será definido no processamento de pagamento
                    Cashier = CurrentUser,
                    DateTime = DateTime.Now
                };

                await _printerService.PrintReceiptAsync(receipt);
                
                MessageBox.Show("Cupom enviado para impressão!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao imprimir cupom");
                MessageBox.Show($"Erro ao imprimir cupom: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnBarcodeScanned(string barcode)
        {
            try
            {
                _logger.LogInformation("Código de barras escaneado: {Barcode}", barcode);
                
                // Buscar produto pelo código de barras
                var product = Products.FirstOrDefault(p => p.Barcode == barcode);
                if (product != null)
                {
                    AddToCart(product);
                }
                else
                {
                    MessageBox.Show($"Produto não encontrado para o código: {barcode}", "Produto Não Encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar código de barras: {Barcode}", barcode);
            }
        }

        private void OnWeightReceived(decimal weight)
        {
            try
            {
                _logger.LogInformation("Peso recebido da balança: {Weight}kg", weight);
                
                // Implementar lógica para produtos pesáveis
                // Por exemplo, atualizar quantidade do último item adicionado
                if (CartItems.Any())
                {
                    var lastItem = CartItems.Last();
                    lastItem.Qty = weight;
                    CalculateCartTotal();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar peso da balança: {Weight}", weight);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
