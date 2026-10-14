using LinkShortener.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LinkShortener.Tests;

public sealed class LinkPersistenceTests
{
    [Fact]
    public async Task MigrationCreatesUniqueIndexAndPersistsLinks()
    {
        var path = Path.Combine(AppContext.BaseDirectory, $"links-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<LinkDbContext>()
            .UseSqlite($"Data Source={path}").Options;

        try
        {
            await using (var db = new LinkDbContext(options))
            {
                await db.Database.MigrateAsync();
                db.Links.Add(new Link { Code = "TEST123", TargetUrl = "https://example.com", CreatedAtUtc = DateTimeOffset.UtcNow });
                await db.SaveChangesAsync();
                db.Links.Add(new Link { Code = "TEST123", TargetUrl = "https://example.org", CreatedAtUtc = DateTimeOffset.UtcNow });
                await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            }

            await using (var reopened = new LinkDbContext(options))
            {
                Assert.Equal("https://example.com", (await reopened.Links.SingleAsync()).TargetUrl);
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(path);
        }
    }
}
