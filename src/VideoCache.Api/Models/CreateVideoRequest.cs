using System.ComponentModel.DataAnnotations;
namespace VideoCache.Api.Models;
public sealed class CreateVideoRequest
{
    [Required(ErrorMessage = "O campo 'id' é obrigatório.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "O 'id' deve ter entre 1 e 100 caracteres.")]
    public string Id { get; set; } = string.Empty;
    [Required(ErrorMessage = "O campo 'url' é obrigatório.")]
    [Url(ErrorMessage = "O campo 'url' deve conter uma URL válida.")]
    public string Url { get; set; } = string.Empty;
}