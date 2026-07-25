using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
namespace VideoCache.Api.Middlewares;
/// <summary>
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }
    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, "Requisição inválida."),
            RedisConnectionException => (HttpStatusCode.ServiceUnavailable, "Cache indisponível no momento."),
            _ => (HttpStatusCode.InternalServerError, "Erro interno no servidor.")
        };
        _logger.LogError(
            exception,
            "Falha ao processar {Method} {Path}",
            context.Request.Method,
            context.Request.Path);
        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Instance = context.Request.Path
        };
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}