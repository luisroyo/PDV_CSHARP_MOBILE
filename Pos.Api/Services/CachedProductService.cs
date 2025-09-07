using Pos.Api.Services;
using Pos.Domain.Entities;
using Pos.Domain.Interfaces;
using System.Collections.Generic;

namespace Pos.Api.Services
{
    public interface ICachedProductService
    {
        Task<IEnumerable<Product>> GetProductsAsync(Guid tenantId);
        Task<Product?> GetProductByIdAsync(Guid id, Guid tenantId);
        Task<Product?> GetProductByBarcodeAsync(string barcode, Guid tenantId);
        Task InvalidateProductCacheAsync(Guid tenantId);
        Task InvalidateProductCacheAsync(Guid productId, Guid tenantId);
    }

    public class CachedProductService : ICachedProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);

        public CachedProductService(IRepository<Product> productRepository, IRedisCacheService cacheService)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync(Guid tenantId)
        {
            var cacheKey = $"products:tenant:{tenantId}";
            
            var cachedProducts = await _cacheService.GetAsync<List<Product>>(cacheKey);
            if (cachedProducts != null)
                return cachedProducts;

            var products = await _productRepository.GetAllAsync();
            var tenantProducts = products.Where(p => p.TenantId == tenantId).ToList();

            await _cacheService.SetAsync(cacheKey, tenantProducts, _cacheExpiry);
            return tenantProducts;
        }

        public async Task<Product?> GetProductByIdAsync(Guid id, Guid tenantId)
        {
            var cacheKey = $"product:{id}:tenant:{tenantId}";
            
            var cachedProduct = await _cacheService.GetAsync<Product>(cacheKey);
            if (cachedProduct != null)
                return cachedProduct;

            var product = await _productRepository.GetByIdAsync(id);
            if (product != null && product.TenantId == tenantId)
            {
                await _cacheService.SetAsync(cacheKey, product, _cacheExpiry);
                return product;
            }

            return null;
        }

        public async Task<Product?> GetProductByBarcodeAsync(string barcode, Guid tenantId)
        {
            var cacheKey = $"product:barcode:{barcode}:tenant:{tenantId}";
            
            var cachedProduct = await _cacheService.GetAsync<Product>(cacheKey);
            if (cachedProduct != null)
                return cachedProduct;

            var products = await _productRepository.GetAllAsync();
            var product = products.FirstOrDefault(p => p.Barcode == barcode && p.TenantId == tenantId);
            
            if (product != null)
            {
                await _cacheService.SetAsync(cacheKey, product, _cacheExpiry);
                return product;
            }

            return null;
        }

        public async Task InvalidateProductCacheAsync(Guid tenantId)
        {
            await _cacheService.RemovePatternAsync($"products:tenant:{tenantId}");
            await _cacheService.RemovePatternAsync($"product:*:tenant:{tenantId}");
        }

        public async Task InvalidateProductCacheAsync(Guid productId, Guid tenantId)
        {
            await _cacheService.RemoveAsync($"product:{productId}:tenant:{tenantId}");
            await _cacheService.RemoveAsync($"product:barcode:*:tenant:{tenantId}");
            await _cacheService.RemoveAsync($"products:tenant:{tenantId}");
        }
    }
}
