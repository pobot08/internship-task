using StackExchange.Redis;
namespace Service3.Proxy.Services;
public class RedisTokenStore : ITokenStore
{
    private readonly IDatabase _db;

    public RedisTokenStore(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public int GetUsed(string key)
    {
        RedisValue value = _db.StringGet(GetRedisKey(key));

        if (!value.HasValue)
            return 0;

        return (int)value;
    }

    public bool TryAdd(string key, int limit, int amount)
    {
        int current = GetUsed(key);

        if (current + amount > limit)
            return false;

        _db.StringSet(GetRedisKey(key), current + amount);

        return true;
    }

    public void Reset(string key)
    {
        _db.StringSet(GetRedisKey(key), 0);
    }

    private static string GetRedisKey(string apiKey)
    {
        return $"tokens:{apiKey}";
    }
}