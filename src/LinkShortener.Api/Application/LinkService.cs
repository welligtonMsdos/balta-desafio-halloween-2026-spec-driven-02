using LinkShortener.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LinkShortener.Api.Application;

public sealed class LinkService(
    ILinkRepository repository,
    CodeGenerator generator,
    TargetUrlValidator validator,
    TimeProvider clock)
{
    public async Task<CreateLinkResult> CreateAsync(string? targetUrl, string? customCode = null,
        CancellationToken cancellationToken = default)
    {
        var error = await validator.ValidateAsync(targetUrl, cancellationToken);
        if (error is not null) return CreateLinkResult.Invalid(error);

        if (customCode is not null && (customCode.Length is < 4 or > 20 ||
            customCode.Any(character => character is not (>= 'A' and <= 'Z' or >= '1' and <= '9'))))
            return CreateLinkResult.Invalid("O código deve ter de 4 a 20 caracteres, usando apenas A-Z e 1-9.");

        if (customCode is not null && await repository.CodeExistsAsync(customCode, cancellationToken))
            return CreateLinkResult.Duplicate();

        var link = new Link
        {
            Code = customCode ?? generator.Generate(),
            TargetUrl = targetUrl!,
            CreatedAtUtc = clock.GetUtcNow()
        };
        try
        {
            await repository.AddAsync(link, cancellationToken);
        }
        catch (DbUpdateException exception) when (customCode is not null &&
            exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            return CreateLinkResult.Duplicate();
        }
        return CreateLinkResult.Success(link);
    }
}
