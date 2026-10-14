using Microsoft.EntityFrameworkCore;

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
        await db.SaveChangesAsync(cancellationToken);
    }
}
