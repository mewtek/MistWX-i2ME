using Microsoft.Extensions.Caching.Memory;

namespace MistWX_i2Me;

public static class Globals
{
    public static IMemoryCache LocationCache = new MemoryCache(new MemoryCacheOptions());
}