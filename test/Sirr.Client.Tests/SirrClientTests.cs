using System.Net;
using System.Text.Json;
using Sirr.Tests.Helpers;

namespace Sirr.Tests;

public sealed class SirrClientTests
{
    private static (SirrClient Client, MockHttpHandler Handler) CreateClient()
    {
        var handler = new MockHttpHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:8080") };
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");
        var client = new SirrClient(http);
        return (client, handler);
    }

    // --- Push ---

    [Fact]
    public async Task PushAsync_SendsCorrectRequest()
    {
        var (client, handler) = CreateClient();
        handler.EnqueueOk(new {
            hash = "abc123",
            url = "http://localhost:8080/secret/abc123",
            expires_at = 1700000000L,
            reads_remaining = 5,
            owned = true
        });

        var result = await client.PushAsync("secret-val", ttl: TimeSpan.FromMinutes(30), reads: 5, prefix: "db_");

        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/secret", request.RequestUri!.AbsolutePath);

        using var doc = JsonDocument.Parse(request.Body!);
        var root = doc.RootElement;
        Assert.Equal("secret-val", root.GetProperty("value").GetString());
        Assert.Equal(1800, root.GetProperty("ttl_seconds").GetInt64());
        Assert.Equal(5, root.GetProperty("reads").GetInt32());
        Assert.Equal("db_", root.GetProperty("prefix").GetString());
        Assert.Equal("abc123", result.Hash);
    }

    // --- Get ---

    [Fact]
    public async Task GetAsync_ReturnsPlaintext()
    {
        var (client, handler) = CreateClient();
        handler.EnqueueOkContent("my-secret");

        var result = await client.GetAsync("abc123");

        Assert.Equal("my-secret", result);
        Assert.Equal("/secret/abc123", handler.Requests[0].RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_On410()
    {
        var (client, handler) = CreateClient();
        handler.EnqueueError(HttpStatusCode.Gone, "Gone");

        var result = await client.GetAsync("abc123");

        Assert.Null(result);
    }

    // --- Inspect ---

    [Fact]
    public async Task InspectAsync_ReturnsStatus()
    {
        var (client, handler) = CreateClient();
        handler.EnqueueHead(HttpStatusCode.OK, new()
        {
            ["X-Sirr-Created"] = "2024-01-15T10:00:00Z",
            ["X-Sirr-Reads-Remaining"] = "5",
            ["X-Sirr-Owned"] = "true"
        });

        var status = await client.InspectAsync("abc123");

        Assert.NotNull(status);
        Assert.Equal(5, status!.ReadsRemaining);
        Assert.True(status.Owned);
        Assert.Equal("2024-01-15T10:00:00Z", status.Created);
    }

    // --- Patch ---

    [Fact]
    public async Task PatchAsync_SendsCorrectRequest()
    {
        var (client, handler) = CreateClient();
        handler.EnqueueOk(new { hash = "abc123", url = "...", owned = true });

        await client.PatchAsync("abc123", value: "new-val", reads: 10);

        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal("/secret/abc123", request.RequestUri!.AbsolutePath);

        using var doc = JsonDocument.Parse(request.Body!);
        var root = doc.RootElement;
        Assert.Equal("new-val", doc.RootElement.GetProperty("value").GetString());
        Assert.Equal(10, doc.RootElement.GetProperty("reads").GetInt32());
    }

    // --- Burn ---

    [Fact]
    public async Task BurnAsync_SendsDelete()
    {
        var (client, handler) = CreateClient();
        handler.EnqueueNoContent();

        await client.BurnAsync("abc123");

        Assert.Equal(HttpMethod.Delete, handler.Requests[0].Method);
        Assert.Equal("/secret/abc123", handler.Requests[0].RequestUri!.AbsolutePath);
    }

    // --- Audit ---

    [Fact]
    public async Task AuditAsync_ReturnsEvents()
    {
        var (client, handler) = CreateClient();
        handler.EnqueueOk(new {
            hash = "abc123",
            created_at = 1700000000L,
            events = new[] {
                new { type = "secret.create", at = 1700000000L, ip = "1.2.3.4" }
            }
        });

        var result = await client.AuditAsync("abc123");

        Assert.Single(result.Events);
        Assert.Equal("secret.create", result.Events[0].Type);
        Assert.Equal("/secret/abc123/audit", handler.Requests[0].RequestUri!.AbsolutePath);
    }

    // --- List ---

    [Fact]
    public async Task ListAsync_ReturnsMetas()
    {
        var (client, handler) = CreateClient();
        handler.EnqueueOk(new[] {
            new {
                hash = "abc123",
                created_at = 1700000000L,
                ttl_expires_at = 1700003600L,
                reads_remaining = 3,
                burned = false,
                owned = true
            }
        });

        var result = await client.ListAsync();

        Assert.Single(result);
        Assert.Equal("abc123", result[0].Hash);
        Assert.Equal(3, result[0].ReadsRemaining);
    }
}
