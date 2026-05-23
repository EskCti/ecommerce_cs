using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RetailOps.Identity.Core.Application.Dtos;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Identity.Infrastructure.Seeds;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

internal static class IntegrationTestUsers
{
    internal static IEnumerable<PermissionGrant> Grants(params string[] keys) =>
        keys.Select(k => PermissionGrant.Create(Guid.NewGuid(), PermissionKey.Create(k).Value));
}

public class AuthEndpointTests : IClassFixture<AuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(AuthWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        AuthWebApplicationFactory.ConfigureUser(null);
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { login = "x", password = "y" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        AuthWebApplicationFactory.ConfigureUser(AuthWebApplicationFactory.CreateAdmin());
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { login = "admin@test.com", password = "secret" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("accessToken", out var token));
        Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
    }

    [Fact]
    public async Task Login_WithInactiveUser_ReturnsForbidden()
    {
        AuthWebApplicationFactory.ConfigureUser(AuthWebApplicationFactory.CreateInactiveUser());
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { login = "inactive@test.com", password = "secret" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithOperatorWithoutGrants_ReturnsForbidden()
    {
        AuthWebApplicationFactory.ConfigureUser(AuthWebApplicationFactory.CreateOperatorWithoutGrants());
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { login = "op@test.com", password = "secret" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithBearer_ReturnsCurrentUserProfile()
    {
        AuthWebApplicationFactory.ConfigureUser(AuthWebApplicationFactory.CreateAdmin());
        var token = await LoginAndGetToken("admin@test.com", "secret");

        var req = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("id", out _));
        Assert.True(body.TryGetProperty("tenantId", out _));
        Assert.True(body.TryGetProperty("userLevel", out _));
        Assert.True(body.TryGetProperty("permissionKeys", out _));
    }

    [Fact]
    public async Task VerifyManagerPin_WithValidPin_ReturnsOk()
    {
        const string pin = "1234";
        var bcrypt = BCrypt.Net.BCrypt.HashPassword(pin);
        AuthWebApplicationFactory.ConfigureUser(AuthWebApplicationFactory.CreateManagerWithPin(bcrypt));

        var token = await LoginAndGetToken("manager@test.com", "secret");
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/verify-manager-pin")
        {
            Content = JsonContent.Create(new { pin }),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task VerifyManagerPin_WithInvalidPin_ReturnsBadRequest()
    {
        var bcrypt = BCrypt.Net.BCrypt.HashPassword("1234");
        AuthWebApplicationFactory.ConfigureUser(AuthWebApplicationFactory.CreateManagerWithPin(bcrypt));

        var token = await LoginAndGetToken("manager@test.com", "secret");
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/verify-manager-pin")
        {
            Content = JsonContent.Create(new { pin = "0000" }),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<string> LoginAndGetToken(string login, string password)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { login, password });
        loginResponse.EnsureSuccessStatusCode();
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return loginBody.GetProperty("accessToken").GetString()!;
    }
}

public class UsersPermissionsEndpointTests : IClassFixture<AuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersPermissionsEndpointTests(AuthWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task PermissionCatalog_Returns35SeedEntries()
    {
        AuthWebApplicationFactory.ConfigureUser(AuthWebApplicationFactory.CreateAdmin());
        var token = await LoginToken("admin@test.com");

        var req = new HttpRequestMessage(HttpMethod.Get, "/api/permissions/catalog");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<PermissionCatalogItemDto>>();
        Assert.NotNull(items);
        Assert.Equal(35, items.Count);
    }

    [Fact]
    public async Task ListUsers_RequiresUsuariosPermission()
    {
        AuthWebApplicationFactory.ConfigureUser(AuthWebApplicationFactory.CreateOperatorWithUsuarios());
        var token = await LoginToken("usuarios@test.com");

        var req = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AssignPermissions_UpdatesUserGrants()
    {
        var targetId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        AuthWebApplicationFactory.ConfigureUser(
            AuthWebApplicationFactory.CreateAdmin(),
            AuthWebApplicationFactory.CreateTargetUser(targetId));

        var token = await LoginToken("admin@test.com");
        var req = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{targetId}/permissions")
        {
            Content = JsonContent.Create(new { permissionKeys = new[] { "produtos", "estoque" } }),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<string> LoginToken(string login)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { login, password = "secret" });
        loginResponse.EnsureSuccessStatusCode();
        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }
}

public sealed class AuthWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid ManagerUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid OperatorUserId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid InactiveUserId = Guid.Parse("00000000-0000-0000-0000-000000000004");
    public static readonly Guid UsuariosUserId = Guid.Parse("00000000-0000-0000-0000-000000000005");

    private static User? _primaryUser;
    private static readonly Dictionary<Guid, User> _usersById = new();

    public static void ConfigureUser(User? primary, User? extra = null)
    {
        _primaryUser = primary;
        _usersById.Clear();
        if (primary is not null)
            _usersById[primary.Id] = primary;
        if (extra is not null)
            _usersById[extra.Id] = extra;
    }

    public static User CreateAdmin() =>
        User.Reconstitute(
            AdminUserId,
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

    public static User CreateManagerWithPin(string bcryptPin) =>
        User.Reconstitute(
            ManagerUserId,
            2,
            TenantId.Create(1).Value,
            "Manager",
            Email.Create("manager@test.com").Value,
            null,
            PasswordHash.CreateBcrypt("$2a$11$abcdefghijklmnopqrstuv").Value,
            ManagerPin.FromBcrypt(bcryptPin).Value,
            UserLevel.Gerente,
            ActiveStatus.Active,
            IntegrationTestUsers.Grants("vendas")).Value;

    public static User CreateOperatorWithoutGrants() =>
        User.Reconstitute(
            OperatorUserId,
            3,
            TenantId.Create(1).Value,
            "Operator",
            Email.Create("op@test.com").Value,
            null,
            PasswordHash.CreateBcrypt("$2a$11$abcdefghijklmnopqrstuv").Value,
            null,
            UserLevel.Operador,
            ActiveStatus.Active,
            []).Value;

    public static User CreateInactiveUser() =>
        User.Reconstitute(
            InactiveUserId,
            4,
            TenantId.Create(1).Value,
            "Inactive",
            Email.Create("inactive@test.com").Value,
            null,
            PasswordHash.CreateBcrypt("$2a$11$abcdefghijklmnopqrstuv").Value,
            null,
            UserLevel.Gerente,
            ActiveStatus.Inactive,
            IntegrationTestUsers.Grants("produtos")).Value;

    public static User CreateOperatorWithUsuarios() =>
        User.Reconstitute(
            UsuariosUserId,
            5,
            TenantId.Create(1).Value,
            "Usuarios",
            Email.Create("usuarios@test.com").Value,
            null,
            PasswordHash.CreateBcrypt("$2a$11$abcdefghijklmnopqrstuv").Value,
            null,
            UserLevel.Operador,
            ActiveStatus.Active,
            IntegrationTestUsers.Grants("usuarios")).Value;

    public static User CreateTargetUser(Guid id) =>
        User.Reconstitute(
            id,
            99,
            TenantId.Create(1).Value,
            "Target",
            Email.Create("target@test.com").Value,
            null,
            PasswordHash.CreateBcrypt("$2a$11$abcdefghijklmnopqrstuv").Value,
            null,
            UserLevel.Operador,
            ActiveStatus.Active,
            IntegrationTestUsers.Grants("vendas")).Value;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var repo = new Mock<IUserRepository>();
            repo.Setup(r => r.FindByEmailOrCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string login, CancellationToken _) =>
                {
                    if (_primaryUser is null) return null;
                    if (login == _primaryUser.Email?.Value || login == _primaryUser.Cpf?.Value)
                        return _primaryUser;
                    return _usersById.Values.FirstOrDefault(u => u.Email?.Value == login || u.Cpf?.Value == login);
                });
            repo.Setup(r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken _) =>
                    _usersById.TryGetValue(id, out var user) ? user : null);
            repo.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(RetailOps.Shared.Kernel.Domain.Results.Result.Success());

            repo.Setup(r => r.ListByTenantAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int _, CancellationToken _) => _usersById.Values.ToList());

            var hasher = new Mock<IPasswordHasher>();
            hasher.Setup(h => h.IsBcryptHash(It.IsAny<string>())).Returns(true);
            hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("$2a$11$newhash");
            hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string plain, string hash) =>
                    plain == "secret" || BCrypt.Net.BCrypt.Verify(plain, hash));

            var catalog = new Mock<IPermissionCatalogQuery>();
            catalog.Setup(c => c.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(PermissionCatalogSeed.Items);

            ReplaceScoped(services, repo.Object);
            ReplaceScoped(services, hasher.Object);
            ReplaceScoped(services, catalog.Object);
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
