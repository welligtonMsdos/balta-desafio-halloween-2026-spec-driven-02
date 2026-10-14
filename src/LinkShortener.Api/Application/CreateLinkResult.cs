using LinkShortener.Api.Data;

namespace LinkShortener.Api.Application;

public sealed record CreateLinkResult(Link? Link, string? Error)
{
    public static CreateLinkResult Success(Link link) => new(link, null);
    public static CreateLinkResult Invalid(string error) => new(null, error);
}
