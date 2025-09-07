using Xunit;
using Pos.Api.Services;
using Pos.Api.Models;

namespace Pos.Tests.Services
{
    public class JwtServiceTests
    {
        [Fact]
        public void GenerateToken_ShouldReturnValidToken()
        {
            // Arrange
            var jwtSettings = new JwtSettings
            {
                SecretKey = "PDV_Multi_Vertical_Super_Secret_Key_2024_At_Least_32_Characters_Long_For_Security",
                Issuer = "PDV-Multi-Vertical",
                Audience = "PDV-Users",
                ExpirationMinutes = 60
            };
            
            var jwtService = new JwtService(jwtSettings);
            var userId = "test-user";
            var username = "testuser";

            // Act
            var token = jwtService.GenerateToken(userId, username);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnValidRefreshToken()
        {
            // Arrange
            var jwtSettings = new JwtSettings
            {
                SecretKey = "PDV_Multi_Vertical_Super_Secret_Key_2024_At_Least_32_Characters_Long_For_Security",
                Issuer = "PDV-Multi-Vertical",
                Audience = "PDV-Users",
                ExpirationMinutes = 60
            };
            
            var jwtService = new JwtService(jwtSettings);

            // Act
            var refreshToken = jwtService.GenerateRefreshToken();

            // Assert
            Assert.NotNull(refreshToken);
            Assert.NotEmpty(refreshToken);
        }
    }
}
