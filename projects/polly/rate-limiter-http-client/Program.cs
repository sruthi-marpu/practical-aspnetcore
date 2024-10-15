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
}).AddResilienceHandler("concurrency-http-policy", (builder, c) =>
{
    builder
        .AddConcurrencyLimiter(permitLimit: 100, queueLimit: 50)
        .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = 3,
            DelayGenerator = static args =>
            {
                var delay = args.AttemptNumber switch
                {
                    0 => TimeSpan.Zero,
                    1 => TimeSpan.FromSeconds(1),
                    _ => TimeSpan.FromSeconds(5)
                };

                // This example uses a synchronous delay generator,
                // but the API also supports asynchronous implementations.
                return new ValueTask<TimeSpan?>(delay);
            },
            OnRetry = args =>
            {
                var logger = c.ServiceProvider.GetService<ILogger>();
                logger.LogError("OnRetry, Attempt: {0}", args.AttemptNumber);
                Console.WriteLine("OnRetry, Attempt: {0}", args.AttemptNumber);

                // Event handlers can be asynchronous; here, we return an empty ValueTask.
                return default;
            },
            BackoffType = DelayBackoffType.Constant
        }).Build();
});

var app = builder.Build();
app.MapRazorPages();
app.Run();