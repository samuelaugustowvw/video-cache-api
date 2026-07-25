using VideoCache.Api.Models;
using VideoCache.Api.Repositories;
namespace VideoCache.Api.Services;
public sealed class VideoService : IVideoService
{
    private readonly IVideoRepository _repository;
    private readonly ILogger<VideoService> _logger;
    public VideoService(IVideoRepository repository, ILogger<VideoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task SaveAsync(CreateVideoRequest request, CancellationToken cancellationToken = default)
    {
        await _repository.SaveAsync(request.Id, request.Url, cancellationToken);
        _logger.LogInformation("Vídeo armazenado no cache. VideoId={VideoId}", request.Id);
    }
    public async Task<VideoResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var url = await _repository.GetUrlAsync(id, cancellationToken);
        if (url is null)
        {
            _logger.LogWarning("Vídeo não encontrado no cache. VideoId={VideoId}", id);
            return null;
        }
        return new VideoResponse(id, url);
    }
}