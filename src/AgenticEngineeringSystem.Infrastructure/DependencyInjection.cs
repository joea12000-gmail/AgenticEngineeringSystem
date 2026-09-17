using AgenticEngineeringSystem.Core.Governance;
using AgenticEngineeringSystem.Core.UrlShortener;
using AgenticEngineeringSystem.Infrastructure.Data;
using AgenticEngineeringSystem.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticEngineeringSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAgenticEngineeringInfrastructure(this IServiceCollection services)
    {
        var connection = new SqliteConnection("Data Source=AgenticEngineeringSystem;Mode=Memory;Cache=Shared");
        connection.Open();

        services.AddSingleton(connection);
        services.AddDbContext<AgenticEngineeringDbContext>((serviceProvider, options) =>
        {
            options.UseSqlite(serviceProvider.GetRequiredService<SqliteConnection>());
        });

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IUrlShortenerService, UrlShortenerService>();
        services.AddSingleton<IPolicyEngine, PolicyEngine>();

        return services;
    }
}
