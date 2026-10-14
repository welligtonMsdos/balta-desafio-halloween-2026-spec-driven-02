using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LinkShortener.Tests;

public sealed class CreateLinkApiTests
{
    [Fact]
    public async Task CreatesAutomaticLink()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/links", new { targetUrl = "https://example.com/page" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var code = body.RootElement.GetProperty("code").GetString();
        Assert.Matches("^[A-Z1-9]{7}$", code!);
        Assert.EndsWith("/" + code, body.RootElement.GetProperty("shortUrl").GetString());
    }
}
