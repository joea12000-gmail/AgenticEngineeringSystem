using AgenticEngineeringSystem.Core.Governance;
using AgenticEngineeringSystem.Core.UrlShortener;
using AgenticEngineeringSystem.Infrastructure;
using AgenticEngineeringSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddAgenticEngineeringInfrastructure();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AgenticEngineeringDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

var urls = app.MapGroup("/api/urls").WithTags("URL Shortener");

urls.MapPost("/", async (
    CreateShortUrlRequest request,
    HttpContext httpContext,
    IUrlShortenerService service,
    CancellationToken cancellationToken) =>
{
    try
    {
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var result = await service.CreateAsync(request, baseUrl, cancellationToken);
        return Results.Created($"/api/urls/{result.ShortCode}", result);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { error = exception.Message });
    }
});

urls.MapGet("/{shortCode}/analytics", async (
    string shortCode,
    IUrlShortenerService service,
    CancellationToken cancellationToken) =>
{
    var analytics = await service.GetAnalyticsAsync(shortCode, cancellationToken);
    return analytics is null ? Results.NotFound() : Results.Ok(analytics);
});

urls.MapDelete("/{shortCode}", async (
    string shortCode,
    IUrlShortenerService service,
    CancellationToken cancellationToken) =>
{
    var deleted = await service.DeleteAsync(shortCode, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.MapGet("/{shortCode}", async (
    string shortCode,
    HttpContext httpContext,
    IUrlShortenerService service,
    CancellationToken cancellationToken) =>
{
    var visitContext = new VisitContext(
        httpContext.Connection.RemoteIpAddress?.ToString(),
        httpContext.Request.Headers.UserAgent.ToString(),
        httpContext.Request.Headers.Referer.ToString());

    var shortUrl = await service.ResolveAsync(shortCode, visitContext, cancellationToken);
    return shortUrl is null ? Results.NotFound() : Results.Redirect(shortUrl.OriginalUrl);
});

var governance = app.MapGroup("/api/governance").WithTags("Governance");

governance.MapGet("/policy", (string action, string target, IPolicyEngine policyEngine) =>
{
    var evaluation = policyEngine.Evaluate(action, target);
    return Results.Ok(evaluation);
});

var workflows = app.MapGroup("/api/workflows").WithTags("Workflow State");

workflows.MapGet("/", async (AgenticEngineeringDbContext dbContext, CancellationToken cancellationToken) =>
{
    var results = await dbContext.Workflows
        .AsNoTracking()
        .Select(workflow => new
        {
            workflow.Id,
            workflow.Name,
            workflow.State,
            workflow.RetryCount,
            workflow.MaxRetries,
            TaskCount = workflow.Tasks.Count
        })
        .ToListAsync(cancellationToken);

    return Results.Ok(results);
});

app.Run();

public partial class Program;
