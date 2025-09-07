using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Pos.Mobile.Maui.Data;
using Pos.Mobile.Maui.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Pos.Mobile.Maui.Services
{
    public class OfflineSyncService
    {
        private readonly LocalDbContext _localDb;
        private readonly ApiService _apiService;
        private readonly ILogger<OfflineSyncService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public OfflineSyncService(LocalDbContext localDb, ApiService apiService, ILogger<OfflineSyncService> logger)
        {
            _localDb = localDb;
            _apiService = apiService;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<bool> SyncProductsAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando sincronização de produtos...");

                // Buscar produtos da API
                var apiProducts = await _apiService.GetProductsAsync();
                if (apiProducts == null || !apiProducts.Any())
                {
                    _logger.LogWarning("Nenhum produto encontrado na API");
                    return false;
                }

                // Limpar produtos locais
                var localProducts = _localDb.Products.ToList();
                _localDb.Products.RemoveRange(localProducts);

                // Adicionar produtos da API
                var cachedProducts = apiProducts.Select(p => new CachedProduct
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Barcode = p.Barcode,
                    Category = p.Category,
                    IsActive = p.IsActive,
                    LastSync = DateTime.UtcNow,
                    JsonData = JsonSerializer.Serialize(p, _jsonOptions)
                }).ToList();

                _localDb.Products.AddRange(cachedProducts);
                await _localDb.SaveChangesAsync();

                _logger.LogInformation("Sincronização de produtos concluída: {Count} produtos", cachedProducts.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar produtos");
                return false;
            }
        }

        public async Task<bool> SyncOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando sincronização de pedidos...");

                // Buscar pedidos da API
                var apiOrders = await _apiService.GetOrdersAsync();
                if (apiOrders == null || !apiOrders.Any())
                {
                    _logger.LogWarning("Nenhum pedido encontrado na API");
                    return false;
                }

                // Limpar pedidos locais
                var localOrders = _localDb.Orders.ToList();
                _localDb.Orders.RemoveRange(localOrders);

                // Adicionar pedidos da API
                var cachedOrders = apiOrders.Select(o => new CachedOrder
                {
                    Id = o.Id,
                    Number = o.Number,
                    Status = o.Status,
                    Total = o.Total,
                    CreatedAt = o.CreatedAt,
                    CustomerName = o.CustomerName,
                    LastSync = DateTime.UtcNow,
                    JsonData = JsonSerializer.Serialize(o, _jsonOptions)
                }).ToList();

                _localDb.Orders.AddRange(cachedOrders);
                await _localDb.SaveChangesAsync();

                _logger.LogInformation("Sincronização de pedidos concluída: {Count} pedidos", cachedOrders.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar pedidos");
                return false;
            }
        }

        public async Task<bool> SyncDashboardDataAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando sincronização de dados do dashboard...");

                // Buscar dados do dashboard da API
                var dashboardData = await _apiService.GetDashboardMetricsAsync();
                if (dashboardData == null)
                {
                    _logger.LogWarning("Dados do dashboard não encontrados na API");
                    return false;
                }

                // Salvar dados do dashboard localmente
                var cachedDashboard = new CachedDashboard
                {
                    Id = 1, // Sempre sobrescrever
                    TotalSales = dashboardData.TodaySales,
                    TotalOrders = dashboardData.TodayOrders,
                    TotalProducts = 0, // Não disponível no DashboardMetricsDto
                    LastSync = DateTime.UtcNow,
                    JsonData = JsonSerializer.Serialize(dashboardData, _jsonOptions)
                };

                // Remover dados antigos
                var existingDashboard = _localDb.Dashboards.FirstOrDefault();
                if (existingDashboard != null)
                {
                    _localDb.Dashboards.Remove(existingDashboard);
                }

                _localDb.Dashboards.Add(cachedDashboard);
                await _localDb.SaveChangesAsync();

                _logger.LogInformation("Sincronização do dashboard concluída");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar dados do dashboard");
                return false;
            }
        }

        public async Task<bool> SyncAllAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando sincronização completa...");

                var productsSuccess = await SyncProductsAsync();
                var ordersSuccess = await SyncOrdersAsync();
                var dashboardSuccess = await SyncDashboardDataAsync();

                var allSuccess = productsSuccess && ordersSuccess && dashboardSuccess;
                
                if (allSuccess)
                {
                    _logger.LogInformation("Sincronização completa concluída com sucesso");
                }
                else
                {
                    _logger.LogWarning("Sincronização completa concluída com alguns erros");
                }

                return allSuccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na sincronização completa");
                return false;
            }
        }

        public async Task<ObservableCollection<ProductDto>> GetCachedProductsAsync()
        {
            try
            {
                var cachedProducts = await _localDb.Products.ToListAsync();
                var products = new ObservableCollection<ProductDto>();

                foreach (var cached in cachedProducts)
                {
                    try
                    {
                        var product = JsonSerializer.Deserialize<ProductDto>(cached.JsonData, _jsonOptions);
                        if (product != null)
                        {
                            products.Add(product);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao deserializar produto {ProductId}", cached.Id);
                    }
                }

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos em cache");
                return new ObservableCollection<ProductDto>();
            }
        }

        public async Task<ObservableCollection<OrderDto>> GetCachedOrdersAsync()
        {
            try
            {
                var cachedOrders = await _localDb.Orders.ToListAsync();
                var orders = new ObservableCollection<OrderDto>();

                foreach (var cached in cachedOrders)
                {
                    try
                    {
                        var order = JsonSerializer.Deserialize<OrderDto>(cached.JsonData, _jsonOptions);
                        if (order != null)
                        {
                            orders.Add(order);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao deserializar pedido {OrderId}", cached.Id);
                    }
                }

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos em cache");
                return new ObservableCollection<OrderDto>();
            }
        }

        public async Task<DashboardDto?> GetCachedDashboardAsync()
        {
            try
            {
                var cachedDashboard = await _localDb.Dashboards.FirstOrDefaultAsync();
                if (cachedDashboard == null)
                {
                    return null;
                }

                return JsonSerializer.Deserialize<DashboardDto>(cachedDashboard.JsonData, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados do dashboard em cache");
                return null;
            }
        }

        public async Task<bool> IsDataStaleAsync(TimeSpan maxAge)
        {
            try
            {
                var lastSync = await _localDb.Products
                    .OrderByDescending(p => p.LastSync)
                    .Select(p => p.LastSync)
                    .FirstOrDefaultAsync();

                if (lastSync == default)
                {
                    return true; // Nenhum dado sincronizado
                }

                return DateTime.UtcNow - lastSync > maxAge;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar idade dos dados");
                return true; // Em caso de erro, considerar dados desatualizados
            }
        }

        public async Task ClearCacheAsync()
        {
            try
            {
                _logger.LogInformation("Limpando cache local...");

                _localDb.Products.RemoveRange(_localDb.Products);
                _localDb.Orders.RemoveRange(_localDb.Orders);
                _localDb.Dashboards.RemoveRange(_localDb.Dashboards);

                await _localDb.SaveChangesAsync();
                _logger.LogInformation("Cache local limpo com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar cache local");
            }
        }
    }
}
