using StackExchange.Redis;
using VideoCache.Api.Middlewares;
using VideoCache.Api.Repositories;
using VideoCache.Api.Services;
var builder = WebApplication.CreateBuilder(args);
// ---------- Logging estruturado em JSON ----------
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});
// ---------- Serviços ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Conexão com o Redis 
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("A connection string 'Redis' não foi configurada.");
var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
redisOptions.AbortOnConnectFail = false;   // não derruba a API se o Redis não subiu
redisOptions.ConnectRetry = 5;
redisOptions.ConnectTimeout = 5000;
builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(redisOptions));
// Classes
builder.Services.AddScoped<IVideoRepository, RedisVideoRepository>();
builder.Services.AddScoped<IVideoService, VideoService>();
// ---------- Health Checks ----------
builder.Services.AddHealthChecks()
    .AddRedis(redisConnectionString, name: "redis", tags: ["ready"]);
var app = builder.Build();
// ---------- Pipeline ----------
app.UseMiddleware<ExceptionHandlingMiddleware>();
// Swagger habilitado
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
// /health/live
app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false
});
// /health/ready
app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
});
app.Run();
public partial class Program { }