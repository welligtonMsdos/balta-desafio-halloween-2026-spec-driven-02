using LinkShortener.Api.Data;

namespace LinkShortener.Api.Application;

public sealed record CreateLinkResult(Link? Link, string? Error, bool Conflict = false)
{
    public static CreateLinkResult Success(Link link) => new(link, null);
    public static CreateLinkResult Invalid(string error) => new(null, error);
    public static CreateLinkResult Duplicate() => new(null, "O código informado já está em uso.", true);
}
