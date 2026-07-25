using Microsoft.AspNetCore.Mvc;
using Moq;
using VideoCache.Api.Controllers;
using VideoCache.Api.Models;
using VideoCache.Api.Services;
namespace VideoCache.UnitTests;
public class CacheControllerTests
{
    private readonly Mock<IVideoService> _service = new();
    private readonly CacheController _controller;
    public CacheControllerTests()
    {
        _controller = new CacheController(_service.Object);
    }
    [Fact]
    public async Task GetById_QuandoNaoExiste_DeveRetornar404()
    {
        _service
            .Setup(s => s.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VideoResponse?)null);

        var resultado = await _controller.GetById("inexistente", CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }
    [Fact]
    public async Task GetById_QuandoExiste_DeveRetornar200()
    {
        _service
            .Setup(s => s.GetByIdAsync("video-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VideoResponse("video-001", "https://youtube.com/watch?v=abc"));
        var resultado = await _controller.GetById("video-001", CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.IsType<VideoResponse>(ok.Value);
    }
}