using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LinkShortener.Tests;

public sealed class StatsApiTests
{
    [Fact]
    public async Task AnyoneKnowingCodeCanReadAccessCount()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/links", new { targetUrl = "https://example.com", code = "PUBLICSTATS" });

        var response = await client.GetAsync("/api/links/PUBLICSTATS/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(0, body.RootElement.GetProperty("accessCount").GetInt64());
        Assert.Equal("PUBLICSTATS", body.RootElement.GetProperty("code").GetString());
    }
}
