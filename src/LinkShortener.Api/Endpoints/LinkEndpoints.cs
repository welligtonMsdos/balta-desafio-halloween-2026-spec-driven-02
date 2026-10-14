using LinkShortener.Api.Application;

namespace LinkShortener.Api.Endpoints;

public static class LinkEndpoints
{
    public static void MapLinkEndpoints(this WebApplication app)
    {
        app.MapGet("/{code}", async (string code, LinkService service, CancellationToken cancellationToken) =>
        {
            var destination = await service.GetDestinationAsync(code, cancellationToken);
            return destination is null ? Results.NotFound() : Results.Redirect(destination);
        });

        app.MapPost("/api/links", async (CreateLinkRequest request, HttpContext context,
            LinkService service, CancellationToken cancellationToken) =>
        {
            var result = await service.CreateAsync(request.TargetUrl, request.Code, cancellationToken);
            if (result.Error is not null)
                return Results.Problem(statusCode: result.Conflict ? 409 :
                    result.Error == "Não foi possível gerar um código único." ? 503 : 400, detail: result.Error);

            var code = result.Link!.Code;
            var shortUrl = $"{context.Request.Scheme}://{context.Request.Host}/{code}";
            return Results.Created(shortUrl, new { shortUrl, code });
        });
    }
}

public sealed record CreateLinkRequest(string? TargetUrl, string? Code = null, DateTimeOffset? ExpiresAtUtc = null);
