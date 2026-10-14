using System.Net;
using System.Net.Http.Json;

namespace LinkShortener.Tests;

public sealed class RedirectApiTests
{
    [Fact]
    public async Task ExistingCodeRedirectsDirectlyAndUnknownCodeReturns404()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        await client.PostAsJsonAsync("/api/links", new { targetUrl = "https://example.com/path", code = "REDIRECT" });

        var redirect = await client.GetAsync("/REDIRECT");
        Assert.Equal(HttpStatusCode.Redirect, redirect.StatusCode);
        Assert.Equal("https://example.com/path", redirect.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/UNKNOWN")).StatusCode);
    }
}
