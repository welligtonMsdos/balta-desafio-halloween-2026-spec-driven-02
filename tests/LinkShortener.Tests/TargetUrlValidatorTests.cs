using System.Net;
using LinkShortener.Api.Application;

namespace LinkShortener.Tests;

public sealed class TargetUrlValidatorTests
{
    private readonly TargetUrlValidator validator = new(new FixedResolver());

    [Theory]
    [InlineData("https://example.com/path")]
    [InlineData("http://example.com")]
    public async Task AcceptsPublicHttpUrls(string url) =>
        Assert.Null(await validator.ValidateAsync(url));

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("https://localhost")]
    [InlineData("http://127.0.0.1")]
    [InlineData("http://192.168.1.1")]
    [InlineData("http://[::1]")]
    [InlineData("https://unresolved.example")]
    public async Task RejectsInvalidOrNonPublicUrls(string url) =>
        Assert.NotNull(await validator.ValidateAsync(url));

    [Fact]
    public async Task RejectsUrlLongerThan2048Characters() =>
        Assert.NotNull(await validator.ValidateAsync("https://example.com/" + new string('a', 2049)));

    private sealed class FixedResolver : IHostAddressResolver
    {
        public Task<IPAddress[]> ResolveAsync(string host, CancellationToken cancellationToken) =>
            Task.FromResult(host == "example.com" ? new[] { IPAddress.Parse("8.8.8.8") } : []);
    }
}
