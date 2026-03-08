using System.Runtime.Caching;

namespace MeetingManagement.Helper;

public static class CacheHelper
{
    private static readonly ObjectCache _cache = MemoryCache.Default;

    public static T Get<T>(string key)
    {
        return (T)_cache.Get(key);
    }


    public static void Set<T>(string key, T value, int minutes = 20)
    {
        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(minutes),
        };

        _cache.Set(key, value, policy);
    }

    public static void Remove(string key)
    {
        _cache.Remove(key);
    }
}