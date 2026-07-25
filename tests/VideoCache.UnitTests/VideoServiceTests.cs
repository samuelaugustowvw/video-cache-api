using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VideoCache.Api.Models;
using VideoCache.Api.Repositories;
using VideoCache.Api.Services;
namespace VideoCache.UnitTests;
public class VideoServiceTests
{
    private readonly Mock<IVideoRepository> _repository = new();
    private readonly VideoService _service;
    public VideoServiceTests()
    {
        _service = new VideoService(
            _repository.Object,
            NullLogger<VideoService>.Instance);
    }
    [Fact]
    public async Task SaveAsync_DeveChamarRepositorioComDadosCorretos()
    {
        var request = new CreateVideoRequest
        {
            Id = "video-001",
            Url = "https://youtube.com/watch?v=abc"
        };
        await _service.SaveAsync(request);
        _repository.Verify(
            r => r.SaveAsync(
                request.Id,
                request.Url,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task GetByIdAsync_QuandoExiste_DeveRetornarVideo()
    {
        _repository
            .Setup(r => r.GetUrlAsync("video-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://youtube.com/watch?v=abc");
        var resultado = await _service.GetByIdAsync("video-001");
        Assert.NotNull(resultado);
        Assert.Equal("video-001", resultado!.Id);
        Assert.Equal("https://youtube.com/watch?v=abc", resultado.Url);
    }
    [Fact]
    public async Task GetByIdAsync_QuandoNaoExiste_DeveRetornarNulo()
    {
        _repository
            .Setup(r => r.GetUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var resultado = await _service.GetByIdAsync("inexistente");
        Assert.Null(resultado);
    }
}