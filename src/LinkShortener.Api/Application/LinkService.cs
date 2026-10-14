using LinkShortener.Api.Data;

namespace LinkShortener.Api.Application;

public sealed class LinkService(
    ILinkRepository repository,
    CodeGenerator generator,
    TargetUrlValidator validator,
    TimeProvider clock)
{
    public async Task<ResolveLinkResult> ResolveAsync(string code, CancellationToken cancellationToken = default)
    {
        var link = await repository.FindByCodeAsync(code, cancellationToken);
        if (link is null) return new(ResolveLinkStatus.Missing);
        var now = clock.GetUtcNow().UtcDateTime;
        if (ExpirationPolicy.IsExpired(link.ExpiresAtUtc, now))
            return new(ResolveLinkStatus.Expired);
        if (!await repository.IncrementAccessCountAsync(code, now, cancellationToken))
            return new(ResolveLinkStatus.Expired);
        return new(ResolveLinkStatus.Found, link.TargetUrl);
    }

    public async Task<long?> GetAccessCountAsync(string code, CancellationToken cancellationToken = default) =>
        (await repository.FindByCodeAsync(code, cancellationToken))?.AccessCount;

    public async Task<CreateLinkResult> CreateAsync(string? targetUrl, string? customCode = null,
        DateTimeOffset? expiresAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        var error = await validator.ValidateAsync(targetUrl, cancellationToken);
        if (error is not null) return CreateLinkResult.Invalid(error);

        if (customCode is not null && (customCode.Length is < 4 or > 20 ||
            customCode.Any(character => character is not (>= 'A' and <= 'Z' or >= '1' and <= '9'))))
            return CreateLinkResult.Invalid("O código deve ter de 4 a 20 caracteres, usando apenas A-Z e 1-9.");

        if (customCode is not null && await repository.CodeExistsAsync(customCode, cancellationToken))
            return CreateLinkResult.Duplicate();

        DateTime? expiry = null;
        if (expiresAtUtc is { } requested)
        {
            expiry = ExpirationPolicy.EndOfUtcDate(requested);
            if (expiry is null)
                return CreateLinkResult.Invalid("A data de expiração é inválida.");
            if (ExpirationPolicy.IsExpired(expiry, clock.GetUtcNow().UtcDateTime))
                return CreateLinkResult.Invalid("A data de expiração deve ser futura.");
        }

        for (var attempt = 0; attempt < 32; attempt++)
        {
            var link = new Link
            {
                Code = customCode ?? generator.Generate(),
                TargetUrl = targetUrl!,
                CreatedAtUtc = clock.GetUtcNow(),
                ExpiresAtUtc = expiry
            };
            try
            {
                await repository.AddAsync(link, cancellationToken);
                return CreateLinkResult.Success(link);
            }
            catch (DuplicateCodeException)
            {
                if (customCode is not null) return CreateLinkResult.Duplicate();
            }
        }
        return CreateLinkResult.Exhausted();
    }
}
