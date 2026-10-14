namespace LinkShortener.Api.Data;

public interface ILinkRepository
{
    Task<Link?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(Link link, CancellationToken cancellationToken = default);
    Task IncrementAccessCountAsync(string code, CancellationToken cancellationToken = default);
}
