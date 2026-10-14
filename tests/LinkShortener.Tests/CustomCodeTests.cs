using System.Net;
using System.Net.Http.Json;

namespace LinkShortener.Tests;

public sealed class CustomCodeTests
{
    [Theory]
    [InlineData("ABCD", HttpStatusCode.Created)]
    [InlineData("ABCDEFGHIJKLMNOPQRST", HttpStatusCode.Created)]
    [InlineData("ABC", HttpStatusCode.BadRequest)]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU", HttpStatusCode.BadRequest)]
    [InlineData("ABC0", HttpStatusCode.BadRequest)]
    [InlineData("abcD", HttpStatusCode.BadRequest)]
    [InlineData("ABC ", HttpStatusCode.BadRequest)]
    [InlineData("ABC!", HttpStatusCode.BadRequest)]
    public async Task ValidatesCustomCode(string code, HttpStatusCode expected)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/links", new { targetUrl = "https://example.com", code });
        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task DuplicateCustomCodeReturnsConflict()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var request = new { targetUrl = "https://example.com", code = "DUPLICATE" };
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/links", request)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/links", request)).StatusCode);
    }

    [Fact]
    public async Task ConcurrentCustomCodeCreationHasSingleWinner()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var request = new { targetUrl = "https://example.com", code = "CONCURRENT" };
        var responses = await Task.WhenAll(Enumerable.Range(0, 2)
            .Select(_ => client.PostAsJsonAsync("/api/links", request)));
        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Conflict);
    }
}
