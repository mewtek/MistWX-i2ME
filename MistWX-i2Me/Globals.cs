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
    /// <summary>
    /// List of the alert detail keys for the AlertsCache, primarily used for clearing
    /// old alerts.
    /// </summary>
    public static List<string> AlertDetailKeys = new();

    /// <summary>
    /// List of geocoordinate strings for parsed locations. Format is latitude,longitude.
    /// New entries are added whenever a new location is stored in memory.
    /// </summary>
    public static List<string> Geocoordinates = new();
}