using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

public class AuthEndpointTests : IClassFixture<AuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(AuthWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { login = "x", password = "y" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { login = "admin@test.com", password = "secret" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("accessToken", out var token));
        Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
    }

    [Fact]
    public async Task Me_WithBearer_ReturnsCurrentUser()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new { login = "admin@test.com", password = "secret" });
        var loginBody = await login.Content.ReadFromJsonAsync<JsonElement>();
        var token = loginBody.GetProperty("accessToken").GetString();

        var req = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public sealed class AuthWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var user = User.Reconstitute(
                TestUserId,
                1,
                TenantId.Create(1).Value,
                "Admin",
                Email.Create("admin@test.com").Value,
                null,
                PasswordHash.CreateBcrypt("$2a$11$abcdefghijklmnopqrstuv").Value,
                null,
                UserLevel.Administrador,
                ActiveStatus.Active,
                []).Value;

            var repo = new Mock<IUserRepository>();
            repo.Setup(r => r.FindByEmailOrCpfAsync("admin@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
            repo.Setup(r => r.FindByIdAsync(TestUserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var hasher = new Mock<IPasswordHasher>();
            hasher.Setup(h => h.IsBcryptHash(It.IsAny<string>())).Returns(true);
            hasher.Setup(h => h.Verify("secret", It.IsAny<string>())).Returns(true);
            hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("$2a$11$newhash");

            ReplaceScoped(services, repo.Object);
            ReplaceScoped(services, hasher.Object);
        });
    }

    private static void ReplaceScoped<T>(IServiceCollection services, T implementation) where T : class
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var d in descriptors)
            services.Remove(d);
        services.AddScoped(_ => implementation);
    }
}
