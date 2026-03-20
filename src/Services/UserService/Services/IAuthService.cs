using UserService.Models;

namespace UserService.Services
{
    public interface IAuthService
    {
        // Hash password using BCrypt (random salt, work factor 11)
        string HashPassword(string password);

        // Verify plain password against stored BCrypt hash
        bool VerifyPassword(string password, string hashedPassword);

        // Generate signed JWT (HS256) with Id, Email, Username, Role claims
        string GenerateJwtToken(User user);
    }
}