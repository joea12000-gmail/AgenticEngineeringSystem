using AgenticEngineeringSystem.Core.UrlShortener;
using AgenticEngineeringSystem.Infrastructure.Data;
using AgenticEngineeringSystem.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgenticEngineeringSystem.Tests;

[TestClass]
public sealed class UrlShortenerServiceTests
{
    private SqliteConnection _connection = null!;
    private AgenticEngineeringDbContext _dbContext = null!;
    private UrlShortenerService _service = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AgenticEngineeringDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new AgenticEngineeringDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        _service = new UrlShortenerService(_dbContext, TimeProvider.System);
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [TestMethod]
    public async Task CreateAsync_WithValidUrl_PersistsShortUrl()
    {
        var result = await _service.CreateAsync(new CreateShortUrlRequest("https://example.com/articles/1", "docs-1"), "https://sho.rt");

        Assert.AreEqual("docs-1", result.ShortCode);
        Assert.AreEqual("https://sho.rt/docs-1", result.ShortUrl);
        Assert.AreEqual(1, await _dbContext.ShortUrls.CountAsync());
    }

    [TestMethod]
    public async Task ResolveAsync_WhenUrlExists_RecordsVisit()
    {
        await _service.CreateAsync(new CreateShortUrlRequest("https://example.com", "abc123"), "https://sho.rt");

        var resolved = await _service.ResolveAsync("abc123", new VisitContext("127.0.0.1", "TestAgent", "https://referrer.example"));

        Assert.IsNotNull(resolved);
        Assert.AreEqual("https://example.com", resolved.OriginalUrl);
        Assert.AreEqual(1, await _dbContext.UrlVisits.CountAsync());
    }

    [TestMethod]
    public async Task DeleteAsync_WhenUrlExists_HidesUrlFromResolution()
    {
        await _service.CreateAsync(new CreateShortUrlRequest("https://example.com", "gone"), "https://sho.rt");

        var deleted = await _service.DeleteAsync("gone");
        var resolved = await _service.ResolveAsync("gone", new VisitContext(null, null, null));

        Assert.IsTrue(deleted);
        Assert.IsNull(resolved);
    }
}
