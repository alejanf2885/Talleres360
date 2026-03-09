using Microsoft.Extensions.Caching.Memory;
using Talleres360.Interfaces.Cache;

namespace Talleres360.Services.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public T? Get<T>(string key)
        {
            _memoryCache.TryGetValue(key, out T? value);
            return value;
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                Priority = CacheItemPriority.High
            };

            _memoryCache.Set(key, value, cacheOptions);
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}