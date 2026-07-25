using StackExchange.Redis;
namespace VideoCache.Api.Repositories;
public sealed class RedisVideoRepository : IVideoRepository
{
    private const string KeyPrefix = "video:";
    private readonly IConnectionMultiplexer _redis;
    public RedisVideoRepository(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }
    public async Task SaveAsync(string id, string url, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(BuildKey(id), url);
    }
    public async Task<string?> GetUrlAsync(string id, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(BuildKey(id));

        return value.IsNullOrEmpty ? null : value.ToString();
    }
    private static string BuildKey(string id) => $"{KeyPrefix}{id}";
}