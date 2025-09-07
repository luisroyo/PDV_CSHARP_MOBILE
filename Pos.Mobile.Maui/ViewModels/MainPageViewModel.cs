using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Pos.Mobile.Maui.Models;
using Pos.Mobile.Maui.Services;
using Pos.Mobile.Maui.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Pos.Mobile.Maui.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;
        private readonly OfflineSyncService _offlineSyncService;
        private readonly ILogger<MainPageViewModel> _logger;
        private ObservableCollection<Models.OrderDto> _orders = new();
        private ObservableCollection<ProductDto> _products = new();
        private Models.OrderDto? _selectedOrder;
        private string _searchText = string.Empty;
        private bool _isRefreshing;
        private string _currentUser = string.Empty;
        private string _businessProfile = string.Empty;
        private decimal _todaySales;
        private int _todayOrders;
        private decimal _averageTicket;

        public MainPageViewModel()
        {
            _apiService = new ApiService();
            _authService = new AuthService();
            
            // Configurar banco de dados local
            var options = new DbContextOptionsBuilder<LocalDbContext>()
                .UseSqlite("Data Source=pos_mobile_offline.db")
                .Options;
            var localDb = new LocalDbContext(options);
            localDb.Database.EnsureCreated();
            
            _offlineSyncService = new OfflineSyncService(
                localDb, 
                _apiService, 
                new Logger<OfflineSyncService>(new LoggerFactory())
            );
            _logger = new Logger<MainPageViewModel>(new LoggerFactory());
            
            Orders = new ObservableCollection<Models.OrderDto>();
            Products = new ObservableCollection<ProductDto>();
            RefreshCommand = new Command(async () => await RefreshDataAsync());
            SearchCommand = new Command(SearchProducts);
            CreateOrderCommand = new Command(async () => await CreateOrderAsync());
            ViewOrderCommand = new Command<Models.OrderDto>(async (order) => await ViewOrderAsync(order));
            SyncCommand = new Command(async () => await SyncDataAsync());

            _ = LoadDataAsync();
            CurrentUser = "Vendedor";
            BusinessProfile = "Farmácia";
        }

        public ObservableCollection<Models.OrderDto> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public ObservableCollection<ProductDto> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public Models.OrderDto? SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
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

        public decimal TodaySales
        {
            get => _todaySales;
            set => SetProperty(ref _todaySales, value);
        }

        public int TodayOrders
        {
            get => _todayOrders;
            set => SetProperty(ref _todayOrders, value);
        }

        public decimal AverageTicket
        {
            get => _averageTicket;
            set => SetProperty(ref _averageTicket, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CreateOrderCommand { get; }
        public ICommand ViewOrderCommand { get; }
        public ICommand SyncCommand { get; }

        private async Task LoadDataAsync()
        {
            try
            {
                // Verificar se os dados estão desatualizados (mais de 5 minutos)
                var isStale = await _offlineSyncService.IsDataStaleAsync(TimeSpan.FromMinutes(5));
                
                if (isStale)
                {
                    _logger.LogInformation("Dados desatualizados, tentando sincronizar...");
                    await SyncDataAsync();
                }
                
                await LoadOrdersAsync();
                await LoadProductsAsync();
                await LoadDashboardDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados");
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
            }
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                var orders = await _offlineSyncService.GetCachedOrdersAsync();
                
                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }
                
                _logger.LogInformation("Carregados {Count} pedidos do cache local", orders.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar pedidos do cache");
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Erro ao carregar pedidos: {ex.Message}", "OK");
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var products = await _offlineSyncService.GetCachedProductsAsync();
                
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
                
                _logger.LogInformation("Carregados {Count} produtos do cache local", products.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar produtos do cache");
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Erro ao carregar produtos: {ex.Message}", "OK");
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                var dashboard = await _offlineSyncService.GetCachedDashboardAsync();
                
                if (dashboard != null)
                {
                    TodaySales = dashboard.TotalSales;
                    TodayOrders = dashboard.TotalOrders;
                    AverageTicket = dashboard.TotalOrders > 0 ? dashboard.TotalSales / dashboard.TotalOrders : 0;
                }
                
                _logger.LogInformation("Dados do dashboard carregados do cache local");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados do dashboard do cache");
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Erro ao carregar métricas: {ex.Message}", "OK");
            }
        }

        private async Task RefreshDataAsync()
        {
            try
            {
                IsRefreshing = true;
                await SyncDataAsync();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar dados");
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Erro ao atualizar dados: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task SyncDataAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando sincronização de dados...");
                var success = await _offlineSyncService.SyncAllAsync();
                
                if (success)
                {
                    _logger.LogInformation("Sincronização concluída com sucesso");
                    await Application.Current?.MainPage?.DisplayAlert("Sucesso", "Dados sincronizados com sucesso!", "OK");
                }
                else
                {
                    _logger.LogWarning("Sincronização concluída com alguns erros");
                    await Application.Current?.MainPage?.DisplayAlert("Aviso", "Sincronização concluída com alguns erros. Verifique a conexão.", "OK");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na sincronização");
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Erro na sincronização: {ex.Message}", "OK");
            }
        }

        private async void SearchProducts()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadProductsAsync();
                    return;
                }

                var allProducts = await _offlineSyncService.GetCachedProductsAsync();
                var filteredProducts = allProducts.Where(p => 
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    p.Barcode?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true
                ).ToList();

                Products.Clear();
                foreach (var product in filteredProducts)
                {
                    Products.Add(product);
                }

                _logger.LogInformation("Busca realizada: {Count} produtos encontrados para '{SearchText}'", filteredProducts.Count, SearchText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos");
                await Application.Current?.MainPage?.DisplayAlert("Erro", $"Erro ao buscar produtos: {ex.Message}", "OK");
            }
        }

        private async Task CreateOrderAsync()
        {
            // TODO: Navegar para tela de criação de pedido
            await Application.Current?.MainPage?.DisplayAlert("Info", "Funcionalidade de criação de pedido será implementada", "OK");
        }

        private async Task ViewOrderAsync(Models.OrderDto order)
        {
            if (order == null) return;
            
            // TODO: Navegar para tela de detalhes do pedido
            await Application.Current?.MainPage?.DisplayAlert("Pedido", $"Visualizando pedido {order.Number}", "OK");
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
