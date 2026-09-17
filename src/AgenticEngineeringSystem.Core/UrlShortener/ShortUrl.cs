namespace AgenticEngineeringSystem.Core.UrlShortener;

public sealed class ShortUrl
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string ShortCode { get; set; }

    public required string OriginalUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<UrlVisit> Visits { get; set; } = new List<UrlVisit>();

    public bool IsExpired(DateTimeOffset now) => ExpiresAt is not null && ExpiresAt <= now;
}
