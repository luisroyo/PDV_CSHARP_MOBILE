using Microsoft.AspNetCore.Mvc;
using Pos.Api.Services;

namespace Pos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly JwtService _jwtService;
        private static readonly Dictionary<string, string> _refreshTokens = new();

        public AuthController(ILogger<AuthController> logger, JwtService jwtService)
        {
            _logger = logger;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Simulação de autenticação - será substituído por lógica real
                await Task.Delay(500);

                if (request.Username == "admin" && request.Password == "admin")
                {
                    var token = _jwtService.GenerateToken(1, "admin", "admin@pdv.com", "Admin");
                    var refreshToken = _jwtService.GenerateRefreshToken();
                    
                    // Armazenar refresh token (em produção, usar Redis ou banco de dados)
                    _refreshTokens[refreshToken] = "admin";

                    return Ok(new LoginResponse
                    {
                        Success = true,
                        Token = token,
                        RefreshToken = refreshToken,
                        User = new UserDto
                        {
                            Id = 1,
                            Login = "admin",
                            Name = "Administrador",
                            Email = "admin@pdv.com",
                            IsActive = true
                        }
                    });
                }

                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Usuário ou senha inválidos"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante login");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Erro interno do servidor"
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                // Simulação de validação de refresh token
                await Task.Delay(200);

                if (string.IsNullOrEmpty(request.RefreshToken) || !_refreshTokens.ContainsKey(request.RefreshToken))
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = "Refresh token inválido"
                    });
                }

                // Gerar novo token
                var token = _jwtService.GenerateToken(1, "admin", "admin@pdv.com", "Admin");
                var refreshToken = _jwtService.GenerateRefreshToken();
                
                // Remover refresh token antigo e adicionar novo
                _refreshTokens.Remove(request.RefreshToken);
                _refreshTokens[refreshToken] = "admin";

                return Ok(new LoginResponse
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante refresh token");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Erro interno do servidor"
                });
            }
        }

    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
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
}