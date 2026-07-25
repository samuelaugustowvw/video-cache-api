using VideoCache.Api.Models;
namespace VideoCache.Api.Services;
public interface IVideoService
{
    Task SaveAsync(CreateVideoRequest request, CancellationToken cancellationToken = default);
    Task<VideoResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}