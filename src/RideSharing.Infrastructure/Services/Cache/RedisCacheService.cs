using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RideSharing.Application.Interfaces;
using StackExchange.Redis;

namespace RideSharing.Infrastructure.Services.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheService> _logger;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

    public RedisCacheService(IConfiguration config, ILogger<RedisCacheService> logger)
    {
        _logger = logger;
        var connStr = config["Redis:ConnectionString"] ?? "localhost:6379";
        var multiplexer = ConnectionMultiplexer.Connect(connStr);
        _db = multiplexer.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue) return null;
        try { return JsonSerializer.Deserialize<T>((string)value!); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize cache key {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiry ?? DefaultTtl);
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }
}
