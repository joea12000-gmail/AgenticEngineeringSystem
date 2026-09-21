using AgenticEngineeringSystem.Core.UrlShortener;
using AgenticEngineeringSystem.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace AgenticEngineeringSystem.Infrastructure.Services;

public sealed class UrlShortenerService(AgenticEngineeringDbContext dbContext, TimeProvider timeProvider) : IUrlShortenerService
{
    public async Task<ShortUrlDto> CreateAsync(CreateShortUrlRequest request, string baseUrl, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out var originalUri) ||
            originalUri.Scheme is not ("http" or "https"))
        {
            throw new ArgumentException("OriginalUrl must be an absolute HTTP or HTTPS URL.", nameof(request));
        }

        var shortCode = string.IsNullOrWhiteSpace(request.CustomCode)
            ? await GenerateUniqueCodeAsync(cancellationToken)
            : request.CustomCode.Trim();

        if (shortCode.Length is < 3 or > 32 || shortCode.Any(character => !char.IsLetterOrDigit(character) && character is not '-' and not '_'))
        {
            throw new ArgumentException("CustomCode must be 3-32 URL-safe characters.", nameof(request));
        }

        var exists = await dbContext.ShortUrls.AnyAsync(url => url.ShortCode == shortCode, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("The requested short code is already in use.");
        }

        var entity = new ShortUrl
        {
            ShortCode = shortCode,
            OriginalUrl = request.OriginalUrl,
            CreatedAt = timeProvider.GetUtcNow(),
            ExpiresAt = request.ExpiresAt
        };

        dbContext.ShortUrls.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity, baseUrl);
    }

    public async Task<ShortUrl?> ResolveAsync(string shortCode, VisitContext visitContext, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var entity = await dbContext.ShortUrls.FirstOrDefaultAsync(url => url.ShortCode == shortCode, cancellationToken);

        if (entity is null || entity.IsDeleted || entity.IsExpired(now))
        {
            return null;
        }

        dbContext.UrlVisits.Add(new UrlVisit
        {
            ShortUrlId = entity.Id,
            VisitedAt = now,
            IpAddress = visitContext.IpAddress,
            UserAgent = visitContext.UserAgent,
            Referrer = visitContext.Referrer
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<UrlAnalyticsDto?> GetAnalyticsAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        var url = await dbContext.ShortUrls
            .Where(u => u.ShortCode == shortCode && !u.IsDeleted)
            .Select(u => new
            {
                u.Id,
                u.ShortCode,
                u.OriginalUrl,
                VisitsCount = u.Visits.Count,
                u.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (url == null) return null;

        var visitedAtList = await dbContext.UrlVisits
            .Where(v => v.ShortUrlId == url.Id)
            .Select(v => (DateTimeOffset?)v.VisitedAt)
            .ToListAsync(cancellationToken);

        var latestVisitedAt = visitedAtList.OrderByDescending(v => v).FirstOrDefault();

        return new UrlAnalyticsDto(
            url.ShortCode,
            url.OriginalUrl,
            url.VisitsCount,
            url.CreatedAt,
            latestVisitedAt);
    }

    public async Task<bool> DeleteAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ShortUrls.FirstOrDefaultAsync(url => url.ShortCode == shortCode, cancellationToken);
        if (entity is null || entity.IsDeleted)
        {
            return false;
        }

        entity.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", string.Empty, StringComparison.Ordinal)
                .Replace("/", string.Empty, StringComparison.Ordinal)
                .Replace("=", string.Empty, StringComparison.Ordinal)[..8];

            if (!await dbContext.ShortUrls.AnyAsync(url => url.ShortCode == code, cancellationToken))
            {
                return code;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique short code.");
    }

    private static ShortUrlDto ToDto(ShortUrl entity, string baseUrl)
    {
        var normalizedBaseUrl = baseUrl.TrimEnd('/');
        return new ShortUrlDto(entity.ShortCode, entity.OriginalUrl, entity.CreatedAt, entity.ExpiresAt, $"{normalizedBaseUrl}/{entity.ShortCode}");
    }
}
