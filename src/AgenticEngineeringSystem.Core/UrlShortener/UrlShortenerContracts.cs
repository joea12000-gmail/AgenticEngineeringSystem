namespace AgenticEngineeringSystem.Core.UrlShortener;

public sealed record CreateShortUrlRequest(string OriginalUrl, string? CustomCode = null, DateTimeOffset? ExpiresAt = null);

public sealed record ShortUrlDto(string ShortCode, string OriginalUrl, DateTimeOffset CreatedAt, DateTimeOffset? ExpiresAt, string ShortUrl);

public sealed record UrlAnalyticsDto(string ShortCode, string OriginalUrl, int VisitCount, DateTimeOffset CreatedAt, DateTimeOffset? LastVisitedAt);

public sealed record VisitContext(string? IpAddress, string? UserAgent, string? Referrer);

public interface IUrlShortenerService
{
    Task<ShortUrlDto> CreateAsync(CreateShortUrlRequest request, string baseUrl, CancellationToken cancellationToken = default);

    Task<ShortUrl?> ResolveAsync(string shortCode, VisitContext visitContext, CancellationToken cancellationToken = default);

    Task<UrlAnalyticsDto?> GetAnalyticsAsync(string shortCode, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string shortCode, CancellationToken cancellationToken = default);
}
