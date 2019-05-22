using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartsUnlimitedWebsite.Models
{
    public class NoMemoryCacheEntry : ICacheEntry
    {
        private object _key;
        private object _value;
        public NoMemoryCacheEntry(object key)
        {
            _key = null;
        }
        public object Key => this._key;

        public object Value { get => this._value; set => this._value = null; }
        public DateTimeOffset? AbsoluteExpiration { get => null; set => _value = null; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get => null; set => _value = null; }
        public TimeSpan? SlidingExpiration { get => null; set => _value = null; }

        public IList<IChangeToken> ExpirationTokens => new List<IChangeToken>();

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks => new List<PostEvictionCallbackRegistration>();

        public CacheItemPriority Priority { get => CacheItemPriority.Low; set => _value = null; }
        public long? Size { get => 1; set => _value = null; }

        public void Dispose()
        {
            
        }
    }
    public class NoMemoryCache : IMemoryCache
    {
        public ICacheEntry CreateEntry(object key)
        {
            return new NoMemoryCacheEntry(key);
        }

        public void Dispose()
        {
            
        }

        public void Remove(object key)
        {
            //nothing to remove
        }

        public bool TryGetValue(object key, out object value)
        {
            value = null;
            //dont find it in cache, ever
            return false;
        }
    }
}
