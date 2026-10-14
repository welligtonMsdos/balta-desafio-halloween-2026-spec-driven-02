using LinkShortener.Api.Application;

namespace LinkShortener.Tests;

public sealed class ExpirationPolicyTests
{
    [Fact]
    public void ConvertsDateToEndOfUtcDayAndKeepsLastInstantValid()
    {
        var expiry = ExpirationPolicy.EndOfUtcDate(
            new DateTimeOffset(2026, 10, 15, 1, 0, 0, TimeSpan.FromHours(1)));

        Assert.Equal(new DateTime(2026, 10, 15, 23, 59, 59, 999).AddTicks(9999), expiry);
        Assert.False(ExpirationPolicy.IsExpired(expiry, expiry!.Value));
        Assert.True(ExpirationPolicy.IsExpired(expiry, expiry.Value.AddTicks(1)));
    }
}
