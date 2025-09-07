using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Pos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private static readonly List<ProductDto> _products = new();

        public ProductsController(ILogger<ProductsController> logger)
        {
            _logger = logger;
            InitializeProducts();
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] string? search = null)
        {
            try
            {
                await Task.Delay(100); // Simula chamada de banco

                var products = _products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    products = products.Where(p => 
                        p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.Sku.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                return Ok(products.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                await Task.Delay(100);

                var product = _products.FirstOrDefault(p => p.Id == id);
                if (product == null)
                {
                    return NotFound();
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produto {ProductId}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                await Task.Delay(200);

                var product = new ProductDto
                {
                    Id = _products.Count + 1,
                    Sku = request.Sku,
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Category = request.Category,
                    Barcode = request.Barcode,
                    Unit = request.Unit,
                    Active = true
                };

                _products.Add(product);

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar produto");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        private void InitializeProducts()
        {
            if (_products.Any()) return;

            var products = new[]
            {
                new ProductDto { Id = 1, Sku = "MED001", Name = "Paracetamol 500mg", Description = "Analgésico e antitérmico", Price = 12.50m, Category = "Medicamentos", Barcode = "7891234567890", Unit = "UN", Active = true },
                new ProductDto { Id = 2, Sku = "MED002", Name = "Ibuprofeno 400mg", Description = "Anti-inflamatório", Price = 15.80m, Category = "Medicamentos", Barcode = "7891234567891", Unit = "UN", Active = true },
                new ProductDto { Id = 3, Sku = "MED003", Name = "Dipirona 500mg", Description = "Analgésico e antitérmico", Price = 8.90m, Category = "Medicamentos", Barcode = "7891234567892", Unit = "UN", Active = true },
                new ProductDto { Id = 4, Sku = "CON001", Name = "Cimento 50kg", Description = "Cimento Portland", Price = 25.00m, Category = "Construção", Barcode = "7891234567893", Unit = "UN", Active = true },
                new ProductDto { Id = 5, Sku = "CON002", Name = "Tijolo Cerâmico", Description = "Tijolo cerâmico comum", Price = 0.85m, Category = "Construção", Barcode = "7891234567894", Unit = "UN", Active = true },
                new ProductDto { Id = 6, Sku = "GRO001", Name = "Arroz 5kg", Description = "Arroz branco tipo 1", Price = 18.90m, Category = "Alimentos", Barcode = "7891234567895", Unit = "UN", Active = true },
                new ProductDto { Id = 7, Sku = "FOO001", Name = "Hambúrguer", Description = "Hambúrguer de carne", Price = 15.00m, Category = "Alimentação", Barcode = "7891234567896", Unit = "UN", Active = true },
                new ProductDto { Id = 8, Sku = "FOO002", Name = "Refrigerante 350ml", Description = "Refrigerante de cola", Price = 4.50m, Category = "Bebidas", Barcode = "7891234567897", Unit = "UN", Active = true }
            };

            _products.AddRange(products);
        }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public bool Active { get; set; }
    }

    public class CreateProductRequest
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
    }
}