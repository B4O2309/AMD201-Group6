using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Models;
using BC = BCrypt.Net.BCrypt;

namespace UserService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// BCrypt generates a unique random salt per call and embeds it in the hash.
        /// This means two calls with the same password produce different hashes —
        /// protecting against Rainbow Table attacks.
        /// Work factor default = 11 (strong enough for production).
        /// </summary>
        public string HashPassword(string password)
        {
            return BC.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BC.Verify(password, hashedPassword);
            }
            catch
            {
                // Return false on any error (e.g. malformed hash) — never throw to caller
                return false;
            }
        }

        /// <summary>
        /// Generates a signed JWT (HS256).
        /// Claims embedded: sub (UserId), email, username, role, jti (unique token ID).
        /// URLService reads these claims directly to authorize requests without calling UserService.
        /// IMPORTANT: JwtSettings:Key must be identical in both UserService and URLService appsettings.
        /// </summary>
        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Key"];

            if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
                throw new Exception("JWT Secret Key must be at least 32 characters for HS256.");

            var key = Encoding.ASCII.GetBytes(secretKey);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("username", user.Username),
                new Claim(ClaimTypes.Role, user.Role) // "User" or "Admin"
            };
            var expiresInHours = int.TryParse(jwtSettings["ExpiresInHours"], out var hours) ? hours : 1;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expiresInHours),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));  
        }
    }
}