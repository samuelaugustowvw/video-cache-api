using Microsoft.AspNetCore.Mvc;
using VideoCache.Api.Models;
using VideoCache.Api.Services;
namespace VideoCache.Api.Controllers;
[ApiController]
[Route("api/cache")]
[Produces("application/json")]
public sealed class CacheController : ControllerBase
{
    private readonly IVideoService _service;
    public CacheController(IVideoService service)
    {
        _service = service;
    }
    /// <summary></summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(
        [FromBody] CreateVideoRequest request,
        CancellationToken cancellationToken)
    {
        await _service.SaveAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = request.Id },
            new VideoResponse(request.Id, request.Url));
    }
    /// <summary></summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var video = await _service.GetByIdAsync(id, cancellationToken);
        return video is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Vídeo não encontrado.",
                Detail = $"Nenhum vídeo encontrado para o id '{id}'."
            })
            : Ok(video);
    }
}