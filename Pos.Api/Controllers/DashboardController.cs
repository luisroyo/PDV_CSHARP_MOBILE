using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Pos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            try
            {
                await Task.Delay(100);

                // Dados mockados para demonstração
                var metrics = new
                {
                    Products = new
                    {
                        Total = 8,
                        Active = 8,
                        Inactive = 0
                    },
                    Orders = new
                    {
                        Total = 15,
                        Today = 3,
                        ThisMonth = 12
                    },
                    Revenue = new
                    {
                        Total = 1250.50m,
                        Today = 89.30m,
                        ThisMonth = 980.75m
                    },
                    RecentOrders = new[]
                    {
                        new { Id = 1, Number = "PED20241206001", Status = "Completed", Total = 45.50m, CreatedAt = DateTime.Now.AddHours(-2), CustomerName = "João Silva" },
                        new { Id = 2, Number = "PED20241206002", Status = "Processing", Total = 78.90m, CreatedAt = DateTime.Now.AddHours(-1), CustomerName = "Maria Santos" },
                        new { Id = 3, Number = "PED20241206003", Status = "Draft", Total = 23.40m, CreatedAt = DateTime.Now.AddMinutes(-30), CustomerName = "Pedro Costa" }
                    },
                    TopProducts = new[]
                    {
                        new { ProductId = 1, ProductName = "Paracetamol 500mg", TotalQuantity = 25, TotalRevenue = 312.50m },
                        new { ProductId = 2, ProductName = "Ibuprofeno 400mg", TotalQuantity = 18, TotalRevenue = 284.40m },
                        new { ProductId = 6, ProductName = "Arroz 5kg", TotalQuantity = 12, TotalRevenue = 226.80m }
                    }
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar métricas do dashboard");
                return StatusCode(500, "Erro interno do servidor");
            }
        }
    }
}