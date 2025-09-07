using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Pos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private static readonly List<OrderDto> _orders = new();
        private static int _nextOrderId = 1;

        public OrdersController(ILogger<OrdersController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] string? status = null)
        {
            try
            {
                await Task.Delay(100);

                var orders = _orders.AsQueryable();

                if (!string.IsNullOrWhiteSpace(status))
                {
                    orders = orders.Where(o => o.Status == status);
                }

                return Ok(orders.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            try
            {
                await Task.Delay(100);

                var order = _orders.FirstOrDefault(o => o.Id == id);
                if (order == null)
                {
                    return NotFound();
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedido {OrderId}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                await Task.Delay(200);

                var orderNumber = $"PED{DateTime.Now:yyyyMMddHHmmss}";
                var total = request.Items.Sum(i => i.Qty * i.UnitPrice);

                var order = new OrderDto
                {
                    Id = _nextOrderId++,
                    Number = orderNumber,
                    Status = "Draft",
                    Total = total,
                    CreatedAt = DateTime.UtcNow,
                    CustomerName = request.CustomerName ?? "Cliente Avulso",
                    Items = request.Items.Select(i => new OrderItemDto
                    {
                        Id = _orders.Count + 1,
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductSku = i.ProductSku,
                        Qty = i.Qty,
                        UnitPrice = i.UnitPrice,
                        Notes = i.Notes
                    }).ToList()
                };

                _orders.Add(order);

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                await Task.Delay(100);

                var order = _orders.FirstOrDefault(o => o.Id == id);
                if (order == null)
                {
                    return NotFound();
                }

                order.Status = request.Status;

                return Ok(new { order.Id, order.Status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do pedido {OrderId}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateOrderRequest
    {
        public string? CustomerName { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateOrderItemRequest
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}