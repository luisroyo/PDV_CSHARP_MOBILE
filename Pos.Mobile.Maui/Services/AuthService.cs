using System.Text;
using System.Text.Json;
using Pos.Mobile.Maui.Models;

namespace Pos.Mobile.Maui.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private string? _token;
        private string? _refreshToken;

        public AuthService()
        {
            _httpClient = new HttpClient();
            _baseUrl = "https://localhost:7000"; // URL da API
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

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
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);

                    if (loginResponse?.Success == true)
                    {
                        _token = loginResponse.Token;
                        _refreshToken = loginResponse.RefreshToken;
                        SetAuthHeader();
                    }

                    return loginResponse;
                }

                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Erro de conexão com o servidor"
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = $"Erro: {ex.Message}"
                };
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_refreshToken))
                    return false;

                var request = new RefreshTokenRequest
                {
                    RefreshToken = _refreshToken
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/auth/refresh", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var refreshResponse = JsonSerializer.Deserialize<RefreshTokenResponse>(responseContent);

                    if (refreshResponse?.Success == true)
                    {
                        _token = refreshResponse.Token;
                        _refreshToken = refreshResponse.RefreshToken;
                        SetAuthHeader();
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public void Logout()
        {
            _token = null;
            _refreshToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private void SetAuthHeader()
        {
            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            }
        }

        public HttpClient GetAuthenticatedClient()
        {
            return _httpClient;
        }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
