using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// ��������� ��������� �����������
builder.Services.AddMemoryCache();

var app = builder.Build();

app.MapGet("/", async (HttpContext context) =>
{
    var html = @"
        <html>
        <body>
            <form method='get' action='/get-cat'>
                <input type='text' name='url' placeholder='Enter URL' />
                <button type='submit'>Submit</button>
            </form>
            <br/>
            <img id='statusCat' src='' alt='' />
        </body>
        </html>";
    await context.Response.WriteAsync(html);
});

app.MapGet("/get-cat", async (HttpContext context, IMemoryCache cache) =>
{
    string url = context.Request.Query["url"];

    HttpClient client = new();
    HttpResponseMessage response;

    try
    {
        response = await client.GetAsync(url);
    }
    catch
    {
        await context.Response.WriteAsync("Error URL");
        return;
    }

    int statusCode = (int)response.StatusCode;
    string cacheKey = $"cat-{statusCode}";

    // ���������, ���� �� ����������� � ����
    if (!cache.TryGetValue(cacheKey, out string catImageUrl))
    {
        catImageUrl = $"https://http.cat/{statusCode}.jpg";

        // ��������� � ��� �������� �� 15 �����
        cache.Set(cacheKey, catImageUrl, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        });
    }

    // ���������� HTML � ���������
    var html = $@"
        <html>
        <body>
            <form method='get' action='/get-cat'>
                <input type='text' name='url' placeholder='Enter URL' />
                <button type='submit'>Submit</button>
            </form>
            <br/>
            <img id='statusCat' src='{catImageUrl}' alt='Cat image' />
        </body>
        </html>";

    await context.Response.WriteAsync(html);
});

app.Run();