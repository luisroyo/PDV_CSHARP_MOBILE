using StackExchange.Redis;
using System.Text.Json;

namespace Pos.Api.Services
{
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
        Task RemoveAsync(string key);
        Task RemovePatternAsync(string pattern);
        Task<bool> ExistsAsync(string key);
        Task<long> IncrementAsync(string key, long value = 1);
        Task<long> DecrementAsync(string key, long value = 1);
    }

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _database = connectionMultiplexer.GetDatabase();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                    return null;

                return JsonSerializer.Deserialize<T>(value, _jsonOptions);
            }
            catch (Exception)
            {
                // Log error but don't throw - cache is not critical
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                await _database.StringSetAsync(key, json, expiry);
            }
            catch (Exception)
            {
                // Log error but don't throw - cache is not critical
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception)
            {
                // Log error but don't throw - cache is not critical
            }
        }

        public async Task RemovePatternAsync(string pattern)
        {
            try
            {
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: pattern);
                await _database.KeyDeleteAsync(keys.ToArray());
            }
            catch (Exception)
            {
                // Log error but don't throw - cache is not critical
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<long> IncrementAsync(string key, long value = 1)
        {
            try
            {
                return await _database.StringIncrementAsync(key, value);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<long> DecrementAsync(string key, long value = 1)
        {
            try
            {
                return await _database.StringDecrementAsync(key, value);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
