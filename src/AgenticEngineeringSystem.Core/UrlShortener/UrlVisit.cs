namespace AgenticEngineeringSystem.Core.UrlShortener;

public sealed class UrlVisit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShortUrlId { get; set; }

    public ShortUrl? ShortUrl { get; set; }

    public DateTimeOffset VisitedAt { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? Referrer { get; set; }
}
