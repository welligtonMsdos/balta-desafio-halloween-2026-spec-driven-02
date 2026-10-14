using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LinkShortener.Tests;

public sealed class ValidationProblemTests
{
    [Fact]
    public async Task InvalidInputsReturnPortugueseProblemDetails()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var invalidUrl = await client.PostAsJsonAsync("/api/links", new { targetUrl = "ftp://example.com" });
        var invalidCode = await client.PostAsJsonAsync("/api/links", new
        {
            targetUrl = "https://example.com", code = "abc"
        });
        var malformedJson = await client.PostAsync("/api/links",
            new StringContent("{bad", Encoding.UTF8, "application/json"));

        await AssertProblemAsync(invalidUrl);
        await AssertProblemAsync(invalidCode);
        await AssertProblemAsync(malformedJson);
    }

    private static async Task AssertProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, body.RootElement.GetProperty("status").GetInt32());
        var detail = body.RootElement.GetProperty("detail").GetString();
        Assert.False(string.IsNullOrWhiteSpace(detail));
        Assert.DoesNotContain("invalid", detail!, StringComparison.OrdinalIgnoreCase);
    }
}
