namespace VideoCache.Api.Repositories;
/// <summary>
/// </summary>
public interface IVideoRepository
{
    Task SaveAsync(string id, string url, CancellationToken cancellationToken = default);
    Task<string?> GetUrlAsync(string id, CancellationToken cancellationToken = default);
}