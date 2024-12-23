using Microsoft.Extensions.Caching.Memory;

namespace MistWX_i2Me;

public static class Globals
{
    /// <summary>
    /// Memory cache for storing LFRecord objects
    /// </summary>
    public static IMemoryCache LocationCache = new MemoryCache(new MemoryCacheOptions());
    /// <summary>
    /// Memory cache for storing AlertHeadline IDs and their UTC expiration times
    /// </summary>
    public static IMemoryCache AlertsCache = new MemoryCache(new MemoryCacheOptions());
}