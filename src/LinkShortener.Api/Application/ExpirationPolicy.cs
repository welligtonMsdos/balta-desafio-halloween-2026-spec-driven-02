namespace LinkShortener.Api.Application;

public static class ExpirationPolicy
{
    public static DateTime? EndOfUtcDate(DateTimeOffset? requested)
    {
        if (requested is null) return null;
        var date = requested.Value.ToUniversalTime().UtcDateTime.Date;
        return date == DateTime.MaxValue.Date ? null : date.AddDays(1).AddTicks(-1);
    }

    public static bool IsExpired(DateTime? expiry, DateTime nowUtc) =>
        expiry is { } value && value < nowUtc;
}
