using StackExchange.Redis;
using System.Text.Json;

public class RedisService
{
    private readonly ConnectionMultiplexer _redis;

    public RedisService()
    {
        _redis = ConnectionMultiplexer.Connect("redis:6379");
    }

    public void SetValue<T>(string key, T value)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(value);
        db.StringSet(key, json);
    }

    public T GetValue<T>(string key)
    {
        var db = _redis.GetDatabase();
        var json = db.StringGet(key);
        return json.HasValue ? JsonSerializer.Deserialize<T>(json) : default;
    }
}
