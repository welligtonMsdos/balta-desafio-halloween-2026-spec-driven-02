using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LinkShortener.Tests;

public sealed class ExpirationTests
{
    [Fact]
    public async Task RemainsValidThroughEndOfUtcDateThenReturns410WithoutCounting()
    {
        var clock = new MutableClock(new DateTimeOffset(2026, 10, 14, 12, 0, 0, TimeSpan.Zero));
        using var factory = new ApiFactory(clock);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var create = await client.PostAsJsonAsync("/api/links", new
        {
            targetUrl = "https://example.com",
            code = "EXPIRES",
            expiresAtUtc = "2026-10-15T00:00:00Z"
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        clock.Now = new DateTimeOffset(2026, 10, 15, 23, 59, 59, TimeSpan.Zero);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/EXPIRES")).StatusCode);
        clock.Now = new DateTimeOffset(2026, 10, 16, 0, 0, 0, TimeSpan.Zero);
        Assert.Equal(HttpStatusCode.Gone, (await client.GetAsync("/EXPIRES")).StatusCode);

        using var stats = JsonDocument.Parse(await client.GetStringAsync("/api/links/EXPIRES/stats"));
        Assert.Equal(1, stats.RootElement.GetProperty("accessCount").GetInt64());
    }

    [Fact]
    public async Task RejectsPastExpirationDate()
    {
        var clock = new MutableClock(new DateTimeOffset(2026, 10, 14, 12, 0, 0, TimeSpan.Zero));
        using var factory = new ApiFactory(clock);
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/links", new
        {
            targetUrl = "https://example.com",
            expiresAtUtc = "2026-10-13T00:00:00Z"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed class MutableClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
