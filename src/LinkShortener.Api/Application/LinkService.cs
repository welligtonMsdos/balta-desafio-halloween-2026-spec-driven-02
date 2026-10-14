using LinkShortener.Api.Data;

namespace LinkShortener.Api.Application;

public sealed class LinkService(
    ILinkRepository repository,
    CodeGenerator generator,
    TargetUrlValidator validator,
    TimeProvider clock)
{
    public async Task<CreateLinkResult> CreateAsync(string? targetUrl, CancellationToken cancellationToken = default)
    {
        var error = await validator.ValidateAsync(targetUrl, cancellationToken);
        if (error is not null) return CreateLinkResult.Invalid(error);

        var link = new Link
        {
            Code = generator.Generate(),
            TargetUrl = targetUrl!,
            CreatedAtUtc = clock.GetUtcNow()
        };
        await repository.AddAsync(link, cancellationToken);
        return CreateLinkResult.Success(link);
    }
}
