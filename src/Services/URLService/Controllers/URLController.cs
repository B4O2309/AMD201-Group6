using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using URLService.Data;
using URLService.Entities;
using URLService.Models;
using URLService.Algorithm;

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

        // POST: api/url/shorten
        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortenURLRequest request)
        {
            // 1. Validate the input URL 
            if (!Uri.TryCreate(request.LongUrl, UriKind.Absolute, out _))
            {
                return BadRequest("Invalid URL format.");
            }

            // 2. Generate unique code using the algorithm
            string shortCode = _algorithm.GenerateCode(request.LongUrl);

            // 3. Check if code already exists in Database
            var existingUrl = await _context.Urls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
            if (existingUrl != null)
            {
                return Ok(new { shortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}" });
            }

            // 4. Store in Database
            var urlEntity = new Url
            {
                LongUrl = request.LongUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.Urls.Add(urlEntity);
            await _context.SaveChangesAsync();

            // 5. Seed cache for new URL
            await _cache.SetStringAsync(shortCode, request.LongUrl, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

            return Created("", new { shortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}" });
        }

        // GET: /{code}
        [HttpGet("/{code}")]
        public async Task<IActionResult> RedirectUrl(string code)
        {
            // 1. Try to get the original URL from Redis Cache 
            string? cachedUrl = await _cache.GetStringAsync(code);

            if (!string.IsNullOrEmpty(cachedUrl))
            {
                _logger.LogInformation("Cache Hit for code: {Code}", code);
                return Redirect(cachedUrl);
            }

            // 2. Cache Miss: Look for the URL in the SQL Database
            var urlEntity = await _context.Urls.FirstOrDefaultAsync(u => u.ShortCode == code);

            if (urlEntity == null)
            {
                return NotFound("URL not found.");
            }

            // 3. Store the result in Redis for future requests 
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            };
            await _cache.SetStringAsync(code, urlEntity.LongUrl, cacheOptions);

            _logger.LogInformation("Cache Miss. Loaded from DB for code: {Code}", code);

            // 4. Redirect users to the original URL
            return Redirect(urlEntity.LongUrl);
        }
    }
}