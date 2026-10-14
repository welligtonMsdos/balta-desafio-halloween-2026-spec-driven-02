using LinkShortener.Api.Application;

namespace LinkShortener.Api.Endpoints;

public static class LinkEndpoints
{
    public static void MapLinkEndpoints(this WebApplication app)
    {
        app.MapPost("/api/links", async (CreateLinkRequest request, HttpContext context,
            LinkService service, CancellationToken cancellationToken) =>
        {
            var result = await service.CreateAsync(request.TargetUrl, cancellationToken);
            if (result.Error is not null)
                return Results.Problem(statusCode: 400, detail: result.Error);

            var code = result.Link!.Code;
            var shortUrl = $"{context.Request.Scheme}://{context.Request.Host}/{code}";
            return Results.Created(shortUrl, new { shortUrl, code });
        });
    }
}

public sealed record CreateLinkRequest(string? TargetUrl, string? Code = null, DateTimeOffset? ExpiresAtUtc = null);
