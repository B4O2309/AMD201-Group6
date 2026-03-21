using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using URLService.Algorithm;
using URLService.Data;
using URLService.Entities;
using URLService.Models;

namespace URLService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class URLController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly IURLShortenerAlgorithm _algorithm;
        private readonly ILogger<URLController> _logger;

        public URLController(
            ApplicationDbContext context,
            IDistributedCache cache,
            IURLShortenerAlgorithm algorithm,
            ILogger<URLController> logger)
        {
            _context = context;
            _cache = cache;
            _algorithm = algorithm;
            _logger = logger;
        }

        // ── Helper: extract UserId from JWT "sub" claim ───────────────────────
        private int GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var id) ? id : 0;
        }

        // =====================================================================
        // USER + ADMIN: Shorten a URL
        // POST: api/url/shorten
        // Requires login (any role). Stores caller's UserId with the URL.
        // =====================================================================
        [Authorize]
        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortenURLRequest request)
        {
            // 1. Validate URL format
            if (!Uri.TryCreate(request.LongUrl, UriKind.Absolute, out _))
                return BadRequest("Invalid URL format.");

            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Could not identify user from token.");

            // 2. Generate short code using Base62 algorithm
            string shortCode = _algorithm.GenerateCode(request.LongUrl);

            // 3. Check if this URL was already shortened by the same user
            var existing = await _context.Urls
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode && u.UserId == userId);

            if (existing != null)
                return Ok(new { shortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}" });

            // 4. Save to DB with UserId for ownership tracking
            var urlEntity = new Url
            {
                LongUrl = request.LongUrl,
                ShortCode = shortCode,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ClickCount = 0
            };

            _context.Urls.Add(urlEntity);
            await _context.SaveChangesAsync();

            // 5. Seed Redis cache for instant redirect next time
            await _cache.SetStringAsync(shortCode, request.LongUrl, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

            return Created("", new { shortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}" });
        }

        // =====================================================================
        // PUBLIC: Redirect short URL → original URL
        // GET: /{code}
        // No auth required — anyone with the short link can use it.
        // =====================================================================
        [HttpGet("/{code}")]
        public async Task<IActionResult> RedirectUrl(string code)
        {
            // 1. Check Redis cache first (fast path)
            string? cachedUrl = await _cache.GetStringAsync(code);

            if (!string.IsNullOrEmpty(cachedUrl))
            {
                _logger.LogInformation("Cache Hit for code: {Code}", code);

                // Always increment click count in DB, even on cache hit
                var entity = await _context.Urls.FirstOrDefaultAsync(u => u.ShortCode == code);
                if (entity != null)
                {
                    entity.ClickCount = (entity.ClickCount ?? 0) + 1;
                    await _context.SaveChangesAsync();
                }

                return Redirect(cachedUrl);
            }

            // 2. Cache miss — query database
            var urlEntity = await _context.Urls.FirstOrDefaultAsync(u => u.ShortCode == code);
            if (urlEntity == null)
                return NotFound("URL not found.");

            // 3. Increment click count
            urlEntity.ClickCount = (urlEntity.ClickCount ?? 0) + 1;
            await _context.SaveChangesAsync();

            // 4. Backfill Redis cache for next request
            await _cache.SetStringAsync(code, urlEntity.LongUrl, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

            _logger.LogInformation("Cache Miss. Loaded from DB for code: {Code}", code);
            return Redirect(urlEntity.LongUrl);
        }

        // =====================================================================
        // USER: View own URLs
        // GET: api/url/my-urls
        // Returns only URLs created by the logged-in user.
        // =====================================================================
        [Authorize]
        [HttpGet("my-urls")]
        public async Task<IActionResult> GetMyUrls()
        {
            var userId = GetCurrentUserId();

            var urls = await _context.Urls
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.LongUrl,
                    u.ShortCode,
                    u.ClickCount,
                    u.CreatedAt,
                    ShortUrl = $"{Request.Scheme}://{Request.Host}/{u.ShortCode}"
                })
                .ToListAsync();

            return Ok(urls);
        }

        // =====================================================================
        // USER: Delete own URL
        // DELETE: api/url/my-urls/{urlId}
        // Users can only delete URLs they own.
        // =====================================================================
        [Authorize]
        [HttpDelete("my-urls/{urlId:int}")]
        public async Task<IActionResult> DeleteMyUrl(int urlId)
        {
            var userId = GetCurrentUserId();

            var url = await _context.Urls
                .FirstOrDefaultAsync(u => u.Id == urlId && u.UserId == userId);

            if (url == null)
                return NotFound(new { message = "URL not found or does not belong to you." });

            // Remove from Redis cache
            await _cache.RemoveAsync(url.ShortCode);

            _context.Urls.Remove(url);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted URL {UrlId}.", userId, urlId);
            return Ok(new { message = "URL deleted successfully." });
        }

        // =====================================================================
        // ADMIN: View all URLs of a specific user
        // GET: api/url/admin/users/{userId}/urls
        // Called by AdminController in UserService (proxy call with admin JWT).
        // =====================================================================
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/users/{userId:int}/urls")]
        public async Task<IActionResult> GetUrlsByUser(int userId)
        {
            var urls = await _context.Urls
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.LongUrl,
                    u.ShortCode,
                    u.ClickCount,
                    u.CreatedAt,
                    ShortUrl = $"{Request.Scheme}://{Request.Host}/{u.ShortCode}"
                })
                .ToListAsync();

            return Ok(urls);
        }

        // =====================================================================
        // ADMIN: Delete any URL regardless of owner
        // DELETE: api/url/admin/urls/{urlId}
        // Called by AdminController in UserService (proxy call with admin JWT).
        // =====================================================================
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/urls/{urlId:int}")]
        public async Task<IActionResult> AdminDeleteUrl(int urlId)
        {
            var url = await _context.Urls.FindAsync(urlId);
            if (url == null)
                return NotFound(new { message = $"URL {urlId} not found." });

            // Remove from Redis cache
            await _cache.RemoveAsync(url.ShortCode);

            _context.Urls.Remove(url);
            await _context.SaveChangesAsync();

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Admin {AdminId} deleted URL {UrlId}.", adminId, urlId);

            return Ok(new { message = $"URL {urlId} deleted successfully." });
        }
    }
}