using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LinkShortener.Tests;

public sealed class RestartPersistenceTests
{
    [Fact]
    public async Task LinkAndAccessCountSurviveApplicationRestart()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"restart-{Guid.NewGuid():N}.db");

        using (var firstApplication = new ApiFactory(databasePath: databasePath))
        using (var client = firstApplication.CreateClient(new() { AllowAutoRedirect = false }))
        {
            Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/links",
                new { targetUrl = "https://example.com/restarted", code = "RESTART" })).StatusCode);
            Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/RESTART")).StatusCode);
        }

        using (var restartedApplication = new ApiFactory(databasePath: databasePath))
        using (var client = restartedApplication.CreateClient(new() { AllowAutoRedirect = false }))
        {
            using var stats = JsonDocument.Parse(await client.GetStringAsync("/api/links/RESTART/stats"));
            Assert.Equal(1, stats.RootElement.GetProperty("accessCount").GetInt64());
            var redirect = await client.GetAsync("/RESTART");
            Assert.Equal(HttpStatusCode.Redirect, redirect.StatusCode);
            Assert.Equal("https://example.com/restarted", redirect.Headers.Location?.ToString());
        }
    }
}
