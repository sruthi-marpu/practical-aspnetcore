var app = WebApplication.Create();
app.MapGet("/", () =>
{
    var html = """
        <!DOCTYPE html>
        <html>
            <head>
                <script src="https://cdn.jsdelivr.net/npm/unpoly@3.8.0/unpoly.min.js"></script>
                    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" crossorigin="anonymous">
                <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/unpoly@3.8.0/unpoly.min.css">
            </head>
            <body>
                <div class="container">
                    <div class="row">
                        <div class="col-md-3">
                            <nav>
                                <a href="/unpoly/1" up-target="article">1st</a>
                            </nav>
                        </div>
                        <div class="col-md-3">
                            <article>Default Content 1</article>
                        </div>
                        <div class="col-md-3">
                            <div id="show" up-hungry>Default Content 2</div>
                        </div>
                    </div>
                </div>
            </body>
        </html>
    """;
    return Results.Content(html, "text/html");
});

app.MapGet("/unpoly/{idx}", (HttpRequest request, int idx) =>
{
    if (request.IsUnpolyJs() is false)
        return Results.Content("");

    return Results.Content($"""
        <article>Tag Selector (article)<strong>{DateTime.UtcNow}</strong></article>
        <div id="show">This is not targeted explicitly but it will show up anyway because the response contains a matching element (#show)</div>
        """);
});

app.Run();


public static class UnpolyJs
{
    public static class Headers
    {
        public const string UpVersion = "X-Up-Version";
    }

    public static bool IsUnpolyJs(this HttpRequest self) => !string.IsNullOrWhiteSpace(UpVersion(self));

    public static string UpVersion(HttpRequest request)
    {
        var header = request.Headers[Headers.UpVersion].ToString();

        return header;
    }
}