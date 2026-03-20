using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;
using UserService.RabbitMQ;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;
        private readonly UserPublisher _publisher;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AppDbContext context,
            IAuthService authService,
            UserPublisher publisher,
            ILogger<AuthController> logger)
        {
            _context = context;
            _authService = authService;
            _publisher = publisher;
            _logger = logger;
        }

        // ── POST: api/auth/register ──────────────────────────────────────────
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Validate role value
            if (request.Role != "User" && request.Role != "Admin")
                return BadRequest(new { message = "Role must be 'User' or 'Admin'." });

            // Check duplicate email
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "Email already exists." });

            // Check duplicate username
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { message = "Username already taken." });

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                HashedPassword = _authService.HashPassword(request.Password), // BCrypt
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Publish async event → URLService's UrlConsumer will receive it
            try
            {
                await _publisher.PublishUserRegisteredAsync(
                    user.Id, user.Email, user.Username, user.Role);
            }
            catch (Exception ex)
            {
                // Non-fatal: registration succeeded even if RabbitMQ is temporarily down
                _logger.LogWarning("[AuthController] Failed to publish user_registered: {Error}", ex.Message);
            }

            return Ok(new { message = "User registered successfully." });
        }

        // ── POST: api/auth/login ─────────────────────────────────────────────
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !_authService.VerifyPassword(request.Password, user.HashedPassword))
                return Unauthorized(new { message = "Invalid email or password." });

            var token = _authService.GenerateJwtToken(user);

            _logger.LogInformation("[AuthController] User {Email} (Role: {Role}) logged in.",
                user.Email, user.Role);

            return Ok(new
            {
                token = token,
                user = new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            });
        }

        // ── GET: api/auth/me ─────────────────────────────────────────────────
        // Returns current user info decoded from JWT claims — no DB call needed
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var username = User.FindFirst("username")?.Value;
            var role = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new { id = userId, email, username, role });
        }
    }
}