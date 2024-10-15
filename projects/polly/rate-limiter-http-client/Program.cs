using Polly;
using Polly.Retry;

var builder = WebApplication.CreateBuilder();
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddConsole();

var services = builder.Services;

services.AddRazorPages();

services.AddHttpClient("concurrency-http")
.ConfigureHttpClient((sp, client) =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
}).AddResilienceHandler("concurrency-http-policy", (builder,c) =>
{
    builder
        .AddConcurrencyLimiter(permitLimit: 100, queueLimit: 50)
        .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Constant
        }).Build();
});

var app  = builder.Build();
app.MapRazorPages();
app.Run();