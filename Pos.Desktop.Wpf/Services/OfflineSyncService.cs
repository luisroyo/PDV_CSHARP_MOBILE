using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pos.Desktop.Wpf.Data;
using Pos.Desktop.Wpf.Models;
using System.Text.Json;

namespace Pos.Desktop.Wpf.Services
{
    public class OfflineSyncService
    {
        private readonly LocalDbContext _localDb;
        private readonly ApiService _apiService;
        private readonly ILogger<OfflineSyncService> _logger;

        public OfflineSyncService(LocalDbContext localDb, ApiService apiService, ILogger<OfflineSyncService> logger)
        {
            _localDb = localDb;
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<bool> IsOnlineAsync()
        {
            try
            {
                var response = await _apiService.GetProductsAsync();
                return response != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task SyncProductsAsync()
        {
            try
            {
                var onlineProducts = await _apiService.GetProductsAsync();
                if (onlineProducts == null) return;

                var localProducts = await _localDb.CachedProducts.ToListAsync();

                foreach (var onlineProduct in onlineProducts)
                {
                    var localProduct = localProducts.FirstOrDefault(p => p.ServerId == onlineProduct.Id);
                    
                    if (localProduct == null)
                    {
                        // Criar novo produto local
                        localProduct = new CachedProduct
                        {
                            ServerId = onlineProduct.Id,
                            Sku = onlineProduct.Sku,
                            Name = onlineProduct.Name,
                            Description = onlineProduct.Description,
                            Price = onlineProduct.Price,
                            Category = onlineProduct.Category,
                            Barcode = onlineProduct.Barcode,
                            Unit = onlineProduct.Unit,
                            Active = onlineProduct.Active,
                            LastSync = DateTime.UtcNow,
                            IsPendingSync = false
                        };
                        _localDb.CachedProducts.Add(localProduct);
                    }
                    else
                    {
                        // Atualizar produto existente
                        localProduct.Sku = onlineProduct.Sku;
                        localProduct.Name = onlineProduct.Name;
                        localProduct.Description = onlineProduct.Description;
                        localProduct.Price = onlineProduct.Price;
                        localProduct.Category = onlineProduct.Category;
                        localProduct.Barcode = onlineProduct.Barcode;
                        localProduct.Unit = onlineProduct.Unit;
                        localProduct.Active = onlineProduct.Active;
                        localProduct.LastSync = DateTime.UtcNow;
                        localProduct.IsPendingSync = false;
                    }
                }

                await _localDb.SaveChangesAsync();
                _logger.LogInformation("Produtos sincronizados com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar produtos");
            }
        }

        public async Task<List<ProductDto>> GetCachedProductsAsync(string? search = null)
        {
            try
            {
                var query = _localDb.CachedProducts.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(p => 
                        p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.Sku.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                var cachedProducts = await query.ToListAsync();
                
                return cachedProducts.Select(p => new ProductDto
                {
                    Id = p.ServerId,
                    Sku = p.Sku,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Category = p.Category,
                    Barcode = p.Barcode,
                    Unit = p.Unit,
                    Active = p.Active
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos em cache");
                return new List<ProductDto>();
            }
        }

        public async Task<OrderDto?> CreateOrderOfflineAsync(CreateOrderRequest request)
        {
            try
            {
                var orderNumber = $"PED{DateTime.Now:yyyyMMddHHmmss}";
                var total = request.Items.Sum(i => i.Qty * i.UnitPrice);

                var cachedOrder = new CachedOrder
                {
                    Number = orderNumber,
                    Status = "Draft",
                    Total = total,
                    CreatedAt = DateTime.UtcNow,
                    CustomerName = request.CustomerName ?? "Cliente Avulso",
                    LastSync = DateTime.UtcNow,
                    IsPendingSync = true
                };

                _localDb.CachedOrders.Add(cachedOrder);
                await _localDb.SaveChangesAsync();

                // Adicionar itens
                foreach (var item in request.Items)
                {
                    var cachedItem = new CachedOrderItem
                    {
                        OrderId = cachedOrder.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductSku = item.ProductSku,
                        Qty = item.Qty,
                        UnitPrice = item.UnitPrice,
                        Notes = item.Notes ?? "",
                        LastSync = DateTime.UtcNow
                    };
                    _localDb.CachedOrderItems.Add(cachedItem);
                }

                await _localDb.SaveChangesAsync();

                // Adicionar à fila de sincronização
                await AddToSyncQueueAsync("Order", "Create", cachedOrder.Id, JsonSerializer.Serialize(request));

                return new OrderDto
                {
                    Id = cachedOrder.Id,
                    Number = cachedOrder.Number,
                    Status = cachedOrder.Status,
                    Total = cachedOrder.Total,
                    CreatedAt = cachedOrder.CreatedAt,
                    CustomerName = cachedOrder.CustomerName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido offline");
                return null;
            }
        }

        public async Task SyncPendingOrdersAsync()
        {
            try
            {
                var pendingOrders = await _localDb.CachedOrders
                    .Where(o => o.IsPendingSync)
                    .Include(o => o.Items)
                    .ToListAsync();

                foreach (var order in pendingOrders)
                {
                    try
                    {
                        var orderRequest = new CreateOrderRequest
                        {
                            CustomerName = order.CustomerName,
                            Items = order.Items.Select(i => new CreateOrderItemRequest
                            {
                                ProductId = i.ProductId,
                                ProductName = i.ProductName,
                                ProductSku = i.ProductSku,
                                Qty = i.Qty,
                                UnitPrice = i.UnitPrice,
                                Notes = i.Notes
                            }).ToList()
                        };

                        var onlineOrder = await _apiService.CreateOrderAsync(orderRequest);
                        
                        if (onlineOrder != null)
                        {
                            order.ServerId = onlineOrder.Id;
                            order.Number = onlineOrder.Number;
                            order.Status = onlineOrder.Status;
                            order.IsPendingSync = false;
                            order.LastSync = DateTime.UtcNow;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao sincronizar pedido {OrderId}", order.Id);
                    }
                }

                await _localDb.SaveChangesAsync();
                _logger.LogInformation("Pedidos pendentes sincronizados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar pedidos pendentes");
            }
        }

        private async Task AddToSyncQueueAsync(string entityType, string operation, int entityId, string entityData)
        {
            var syncItem = new SyncQueue
            {
                EntityType = entityType,
                Operation = operation,
                EntityId = entityId,
                EntityData = entityData,
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            };

            _localDb.SyncQueue.Add(syncItem);
            await _localDb.SaveChangesAsync();
        }

        public async Task ProcessSyncQueueAsync()
        {
            try
            {
                var pendingItems = await _localDb.SyncQueue
                    .Where(s => s.RetryCount < 3)
                    .OrderBy(s => s.CreatedAt)
                    .ToListAsync();

                foreach (var item in pendingItems)
                {
                    try
                    {
                        // Processar item da fila
                        // Implementar lógica específica para cada tipo de entidade
                        
                        item.RetryCount++;
                        item.LastRetryAt = DateTime.UtcNow;
                        
                        if (item.RetryCount >= 3)
                        {
                            item.ErrorMessage = "Máximo de tentativas excedido";
                        }
                    }
                    catch (Exception ex)
                    {
                        item.ErrorMessage = ex.Message;
                        _logger.LogError(ex, "Erro ao processar item da fila de sincronização {ItemId}", item.Id);
                    }
                }

                await _localDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar fila de sincronização");
            }
        }
    }
}
