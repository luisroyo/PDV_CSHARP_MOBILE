using System.Text;
using System.Text.Json;
using Pos.Mobile.Maui.Models;

namespace Pos.Mobile.Maui.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _baseUrl = "http://localhost:5071/api";
        }

        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    Username = username,
                    Password = password
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/auth/login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<LoginResponse>(responseContent);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<ProductDto>?> GetProductsAsync(string? search = null)
        {
            try
            {
                var url = $"{_baseUrl}/products";
                if (!string.IsNullOrEmpty(search))
                {
                    url += $"?search={Uri.EscapeDataString(search)}";
                }

                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<ProductDto>>(content);
                }

                return new List<ProductDto>();
            }
            catch (Exception)
            {
                return new List<ProductDto>();
            }
        }

        public async Task<List<OrderDto>?> GetOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/orders");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<OrderDto>>(content);
                }

                return new List<OrderDto>();
            }
            catch (Exception)
            {
                return new List<OrderDto>();
            }
        }

        public async Task<DashboardMetricsDto?> GetDashboardMetricsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/dashboard/metrics");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<DashboardMetricsDto>(content);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public UserDto? User { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }


    public class DashboardMetricsDto
    {
        public decimal TodaySales { get; set; }
        public int TodayOrders { get; set; }
        public decimal AverageTicket { get; set; }
        public List<TopProductDto> TopProducts { get; set; } = new();
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        public List<SalesByHourDto> SalesByHour { get; set; } = new();
    }

    public class TopProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RecentOrderDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class SalesByHourDto
    {
        public int Hour { get; set; }
        public decimal Sales { get; set; }
    }
}
