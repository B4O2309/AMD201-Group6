using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Data;
using UserService.DTOs;
using UserService.RabbitMQ;

namespace UserService.Controllers
{
    /// <summary>
    /// Admin-only endpoints. All routes require Role = "Admin" in the JWT token.
    ///
    /// Capabilities:
    ///   GET    api/admin/users                         → View all users
    ///   DELETE api/admin/users/{userId}                → Delete a user account
    ///   GET    api/admin/users/{userId}/urls            → View all URLs of a user (proxied to URLService)
    ///   DELETE api/admin/urls/{urlId}                  → Delete any URL (proxied to URLService)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserPublisher _publisher;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            AppDbContext context,
            UserPublisher publisher,
            IHttpClientFactory httpClientFactory,
            ILogger<AdminController> logger)
        {
            _context = context;
            _publisher = publisher;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ── GET: api/admin/users ─────────────────────────────────────────────
        // Admin sees the full list of all registered users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .OrderBy(u => u.CreatedAt)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // ── DELETE: api/admin/users/{userId} ─────────────────────────────────
        // Admin deletes a user account; publishes user_deleted event to RabbitMQ
        [HttpDelete("users/{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Prevent admin from deleting their own account
            if (adminId == userId.ToString())
                return BadRequest(new { message = "Admin cannot delete their own account." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = $"User {userId} not found." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Notify URLService asynchronously — can be used to flag/clean up that user's URLs
            try
            {
                await _publisher.PublishUserDeletedAsync(user.Id, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[AdminController] Failed to publish user_deleted: {Error}", ex.Message);
            }

            _logger.LogInformation("[AdminController] Admin {AdminId} deleted user {UserId}.",
                adminId, userId);

            return Ok(new { message = $"User '{user.Username}' deleted successfully." });
        }

        // ── GET: api/admin/users/{userId}/urls ───────────────────────────────
        // Admin views all URLs created by a specific user — proxied to URLService
        [HttpGet("users/{userId:int}/urls")]
        public async Task<IActionResult> GetUrlsByUser(int userId)
        {
            // Verify the target user exists in UserService DB
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return NotFound(new { message = $"User {userId} not found." });

            try
            {
                var client = _httpClientFactory.CreateClient("URLService");

                // Forward admin's JWT so URLService can authorize the [Authorize(Roles="Admin")] endpoint
                var token = Request.Headers["Authorization"].ToString();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);

                var response = await client.GetAsync($"/api/url/admin/users/{userId}/urls");

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode,
                        new { message = "Failed to fetch URLs from URLService." });

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError("[AdminController] URLService call failed: {Error}", ex.Message);
                return StatusCode(503, new { message = "URLService is currently unavailable." });
            }
        }

        // ── DELETE: api/admin/urls/{urlId} ───────────────────────────────────
        // Admin deletes any URL regardless of who created it — proxied to URLService
        [HttpDelete("urls/{urlId:int}")]
        public async Task<IActionResult> DeleteUrl(int urlId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("URLService");
                var token = Request.Headers["Authorization"].ToString();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);

                var response = await client.DeleteAsync($"/api/url/admin/urls/{urlId}");

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode,
                        new { message = "Failed to delete URL in URLService." });

                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("[AdminController] Admin {AdminId} deleted URL {UrlId}.",
                    adminId, urlId);

                return Ok(new { message = $"URL {urlId} deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError("[AdminController] URLService call failed: {Error}", ex.Message);
                return StatusCode(503, new { message = "URLService is currently unavailable." });
            }
        }
    }
}