using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel(IHttpClientFactory clientFactory, ILogger<IndexModel> logger) : PageModel
{
    public ConcurrentDictionary<int, string> Track { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        var client = clientFactory.CreateClient("concurrency-http");

        var url = "https://dummyjson.com/products";
        var totalRequests = 1000;

        const int batchSize = 10;
        for (int i = 0; i < totalRequests; i += batchSize)
        {
            var batchTasks = new List<Task>();
            foreach (var x in Enumerable.Range(i, Math.Min(batchSize, totalRequests - i)))
            {
                var batchNo = i == 0 ? 0 : i / batchSize;
                batchTasks.Add(LoadProductsJsonAsync(client, logger, batchNo, x, url, Track));
            }
            await Task.WhenAll(batchTasks);
        }

        return Page();
    }
    static async Task LoadProductsJsonAsync(HttpClient client, ILogger log, int batch, int idx, string url, ConcurrentDictionary<int, string> track)
    {
        var result = await client.GetAsync(url);
        log.LogInformation($"Batch {batch} Request {idx} completed with status code {result.StatusCode}");
        track[idx] = $"In Batch {batch} completed with status code {result.StatusCode}";
    }
}