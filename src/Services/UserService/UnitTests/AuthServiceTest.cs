using Microsoft.Extensions.Configuration;
using UserService.Models;
using UserService.Services;
using Xunit;

namespace UserService.UnitTests
{
    public class AuthServiceTests
    {
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // In-memory config — mirrors appsettings.json structure
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:Key"] = "SuperSecretKey_AtLeast32Characters_ForHS256!",
                    ["JwtSettings:Issuer"] = "UserService",
                    ["JwtSettings:Audience"] = "URLShortenerApp",
                    ["JwtSettings:ExpiresInDays"] = "7"
                })
                .Build();

            _authService = new AuthService(config);
        }

        // ── BCrypt Tests ──────────────────────────────────────────────────────

        [Fact]
        public void HashPassword_ShouldReturnNonEmptyHash()
        {
            var hash = _authService.HashPassword("myPassword123");
            Assert.False(string.IsNullOrEmpty(hash));
        }

        [Fact]
        public void HashPassword_SamePassword_ShouldReturnDifferentHashes()
        {
            // BCrypt uses a unique random salt per call — two hashes must differ
            var hash1 = _authService.HashPassword("myPassword123");
            var hash2 = _authService.HashPassword("myPassword123");
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ShouldReturnTrue()
        {
            var hash = _authService.HashPassword("correct_password");
            Assert.True(_authService.VerifyPassword("correct_password", hash));
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ShouldReturnFalse()
        {
            var hash = _authService.HashPassword("correct_password");
            Assert.False(_authService.VerifyPassword("wrong_password", hash));
        }

        [Fact]
        public void VerifyPassword_InvalidHash_ShouldReturnFalse()
        {
            // Must NOT throw — AuthService catches exceptions internally
            Assert.False(_authService.VerifyPassword("password", "not_a_valid_bcrypt_hash"));
        }

        // ── JWT Tests ─────────────────────────────────────────────────────────

        [Fact]
        public void GenerateJwtToken_ValidUser_ShouldReturnNonEmptyToken()
        {
            var user = new User { Id = 1, Username = "toan", Email = "toan@example.com", Role = "User" };
            var token = _authService.GenerateJwtToken(user);
            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public void GenerateJwtToken_ShouldHaveThreeParts()
        {
            // JWT format: header.payload.signature
            var user = new User { Id = 2, Username = "bao", Email = "bao@example.com", Role = "Admin" };
            var parts = _authService.GenerateJwtToken(user).Split('.');
            Assert.Equal(3, parts.Length);
        }

        [Fact]
        public void GenerateJwtToken_TwoCalls_ShouldReturnDifferentTokens()
        {
            // Jti is a new Guid each call — tokens differ even for same user
            var user = new User { Id = 3, Username = "hai", Email = "hai@example.com", Role = "User" };
            var token1 = _authService.GenerateJwtToken(user);
            var token2 = _authService.GenerateJwtToken(user);
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void GenerateJwtToken_AdminRole_ShouldSucceed()
        {
            var admin = new User { Id = 4, Username = "admin", Email = "admin@example.com", Role = "Admin" };
            var token = _authService.GenerateJwtToken(admin);
            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public void GenerateJwtToken_ShortSecretKey_ShouldThrowException()
        {
            var weakConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:Key"] = "tooshort"
                })
                .Build();

            var weakService = new AuthService(weakConfig);
            var user = new User { Id = 5, Username = "test", Email = "test@test.com", Role = "User" };

            Assert.Throws<Exception>(() => weakService.GenerateJwtToken(user));
        }
    }
}