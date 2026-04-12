using System.Net;
using System.Text;
using System.Text.Json;
using Sirr.Tests.Helpers;

namespace Sirr.Tests;

public sealed class E2ETests
{
    private sealed class StatefulHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, (string Value, int Reads)> _secrets = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var path = request.RequestUri!.AbsolutePath;
            
            if (request.Method == HttpMethod.Post && path == "/secret")
            {
                using var doc = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
                var val = doc.RootElement.GetProperty("value").GetString()!;
                var reads = doc.RootElement.TryGetProperty("reads", out var r) ? r.GetInt32() : 1;
                var h = $"hash_{val}";
                _secrets[h] = (val, reads);
                var json = JsonSerializer.Serialize(new { hash = h, url = $"http://test/s/{h}", owned = true });
                return new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
            }

            if (request.Method == HttpMethod.Get && path.StartsWith("/secret/", StringComparison.Ordinal))
            {
                var h = path.Substring("/secret/".Length);
                if (!_secrets.TryGetValue(h, out var s) || s.Reads <= 0)
                    return new HttpResponseMessage(HttpStatusCode.Gone);

                _secrets[h] = (s.Value, s.Reads - 1);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(s.Value, Encoding.UTF8, "text/plain") };
            }

            if (request.Method == HttpMethod.Delete && path.StartsWith("/secret/", StringComparison.Ordinal))
            {
                var h = path.Substring("/secret/".Length);
                _secrets.Remove(h);
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task FullFlowSimulation()
    {
        var handler = new StatefulHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new SirrClient(http);

        // 1. Push
        var res = await client.PushAsync("hello", reads: 2);
        Assert.Equal("hash_hello", res.Hash);

        // 2. Get 1
        Assert.Equal("hello", await client.GetAsync(res.Hash));

        // 3. Get 2 (burns after)
        Assert.Equal("hello", await client.GetAsync(res.Hash));

        // 4. Get 3 (gone)
        Assert.Null(await client.GetAsync(res.Hash));

        // 5. Burn manually
        var res2 = await client.PushAsync("burn-me");
        await client.BurnAsync(res2.Hash);
        Assert.Null(await client.GetAsync(res2.Hash));
    }
}
