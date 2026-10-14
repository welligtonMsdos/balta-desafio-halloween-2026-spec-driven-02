using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace LinkShortener.Api.Data;

public sealed class EfLinkRepository(LinkDbContext db) : ILinkRepository
{
    public Task<Link?> FindByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        db.Links.AsNoTracking().SingleOrDefaultAsync(link => link.Code == code, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        db.Links.AnyAsync(link => link.Code == code, cancellationToken);

    public async Task AddAsync(Link link, CancellationToken cancellationToken = default)
    {
        db.Links.Add(link);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException
            { SqliteErrorCode: 19, SqliteExtendedErrorCode: 2067 })
        {
            db.Entry(link).State = EntityState.Detached;
            throw new DuplicateCodeException();
        }
    }
}
