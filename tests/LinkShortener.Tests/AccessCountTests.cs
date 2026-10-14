using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LinkShortener.Tests;

public sealed class AccessCountTests
{
    [Fact]
    public async Task ConcurrentRepeatedAndAutomatedAccessesAreAllCounted()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        await client.PostAsJsonAsync("/api/links", new { targetUrl = "https://example.com", code = "COUNTING" });

        var requests = Enumerable.Range(0, 20).Select(async index =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/COUNTING");
            if (index % 2 == 0) request.Headers.UserAgent.ParseAdd("TestBot/1.0");
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        });
        await Task.WhenAll(requests);

        using var body = JsonDocument.Parse(await client.GetStringAsync("/api/links/COUNTING/stats"));
        Assert.Equal(20, body.RootElement.GetProperty("accessCount").GetInt64());
    }
}
