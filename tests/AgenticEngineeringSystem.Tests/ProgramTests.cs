using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using AgenticEngineeringSystem.Core.UrlShortener;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using AgenticEngineeringSystem.Infrastructure.Data;

using Moq;

namespace AgenticEngineeringSystem.Api.Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public async Task CreateShortUrl_ReturnsCreated()
        {
            var urlDto = new ShortUrlDto("abc", "https://example.com", DateTimeOffset.UtcNow, null, "http://localhost/api/urls/abc");
            var mockService = new Mock<IUrlShortenerService>();
            mockService
                .Setup(s => s.CreateAsync(It.IsAny<CreateShortUrlRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(urlDto);

            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    // override URL shortener service
                    services.AddSingleton<IUrlShortenerService>(mockService.Object);

                    // use unique in-memory sqlite per test to avoid cross-test conflicts
                    var name = $"AgenticEngineeringSystem_{Guid.NewGuid():N}";
                    services.AddSingleton(sp =>
                    {
                        var conn = new SqliteConnection($"Data Source={name};Mode=Memory;Cache=Shared");
                        conn.Open();
                        return conn;
                    });

                    services.AddDbContext<AgenticEngineeringDbContext>((sp, options) =>
                    {
                        var conn = sp.GetRequiredService<SqliteConnection>();
                        options.UseSqlite(conn);
                    });
                })
            );

            var client = factory.CreateClient();
            var request = new CreateShortUrlRequest("https://example.com");
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/urls/", content);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            // Location header should point to the created short url
            Assert.IsTrue(response.Headers.Location?.ToString().Contains("/api/urls/abc") == true);
        }

        [TestMethod]
        public async Task CreateShortUrl_ReturnsBadRequest_OnArgumentException()
        {
            var mockService = new Mock<IUrlShortenerService>();
            mockService
                .Setup(s => s.CreateAsync(It.IsAny<CreateShortUrlRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("invalid url"));

            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IUrlShortenerService>(mockService.Object);

                    var name = $"AgenticEngineeringSystem_{Guid.NewGuid():N}";
                    services.AddSingleton(sp =>
                    {
                        var conn = new SqliteConnection($"Data Source={name};Mode=Memory;Cache=Shared");
                        conn.Open();
                        return conn;
                    });

                    services.AddDbContext<AgenticEngineeringDbContext>((sp, options) =>
                    {
                        var conn = sp.GetRequiredService<SqliteConnection>();
                        options.UseSqlite(conn);
                    });
                })
            );

            var client = factory.CreateClient();
            var request = new CreateShortUrlRequest("not-a-url");
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/urls/", content);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(body.IndexOf("invalid url", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [TestMethod]
        public async Task Redirect_ReturnsRedirect_WhenShortUrlFound()
        {
            var shortUrl = new ShortUrl
            {
                ShortCode = "abc",
                OriginalUrl = "https://example.com"
            };

            var mockService = new Mock<IUrlShortenerService>();
            mockService
                .Setup(s => s.ResolveAsync(It.IsAny<string>(), It.IsAny<VisitContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shortUrl);

            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IUrlShortenerService>(mockService.Object);

                    var name = $"AgenticEngineeringSystem_{Guid.NewGuid():N}";
                    services.AddSingleton(sp =>
                    {
                        var conn = new SqliteConnection($"Data Source={name};Mode=Memory;Cache=Shared");
                        conn.Open();
                        return conn;
                    });

                    services.AddDbContext<AgenticEngineeringDbContext>((sp, options) =>
                    {
                        var conn = sp.GetRequiredService<SqliteConnection>();
                        options.UseSqlite(conn);
                    });
                })
            );

            var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var response = await client.GetAsync("/abc");

            Assert.IsTrue(response.StatusCode == HttpStatusCode.Redirect ||
                          response.StatusCode == HttpStatusCode.Found ||
                          response.StatusCode == HttpStatusCode.RedirectMethod);
            // Accept either with or without trailing slash
            var location = response.Headers.Location?.ToString()?.TrimEnd('/');
            Assert.AreEqual("https://example.com", location);
        }

        [TestMethod]
        public async Task Redirect_ReturnsNotFound_WhenShortUrlMissing()
        {
            var mockService = new Mock<IUrlShortenerService>();
            mockService
                .Setup(s => s.ResolveAsync(It.IsAny<string>(), It.IsAny<VisitContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShortUrl?)null);

            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => services.AddSingleton<IUrlShortenerService>(mockService.Object))
            );

            var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var response = await client.GetAsync("/missing-code");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}