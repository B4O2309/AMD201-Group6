using Microsoft.Extensions.Caching.Distributed;

namespace URLService.Services
{
    public class CacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Get value from Redis by key
        public async Task<string?> GetCacheAsync(string key)
        {
            return await _cache.GetStringAsync(key);
        }

        // Set value to Redis with 24-hour expiration
        public async Task SetCacheAsync(string key, string value)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            };
            await _cache.SetStringAsync(key, value, options);
        }

        // Remove a specific key from cache
        public async Task RemoveCacheAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}