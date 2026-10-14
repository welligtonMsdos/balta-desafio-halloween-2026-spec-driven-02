using System.Net;
using LinkShortener.Api.Application;
using LinkShortener.Api.Data;

namespace LinkShortener.Tests;

public sealed class AutomaticCollisionTests
{
    [Fact]
    public async Task RetriesAfterUniqueIndexCollision()
    {
        var repository = new CollidingRepository();
        var service = new LinkService(repository, new CodeGenerator(new SequenceRandom()),
            new TargetUrlValidator(new PublicResolver()), TimeProvider.System);

        var result = await service.CreateAsync("https://example.com");

        Assert.Null(result.Error);
        Assert.Equal("BBBBBBB", result.Link!.Code);
        Assert.Equal(2, repository.Attempts);
    }

    private sealed class CollidingRepository : ILinkRepository
    {
        public int Attempts { get; private set; }
        public Task<Link?> FindByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult<Link?>(null);
        public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
        public Task<bool> IncrementAccessCountAsync(string code, DateTime nowUtc,
            CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task AddAsync(Link link, CancellationToken cancellationToken = default)
        {
            Attempts++;
            if (Attempts == 1) throw new DuplicateCodeException();
            return Task.CompletedTask;
        }
    }

    private sealed class SequenceRandom : ICodeRandom
    {
        private int count;
        public int Next(int exclusiveMax) => count++ < 7 ? 0 : 1;
    }

    private sealed class PublicResolver : IHostAddressResolver
    {
        public Task<IPAddress[]> ResolveAsync(string host, CancellationToken cancellationToken) =>
            Task.FromResult(new[] { IPAddress.Parse("8.8.8.8") });
    }
}
