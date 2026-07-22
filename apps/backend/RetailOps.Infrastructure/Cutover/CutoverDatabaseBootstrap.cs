using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetailOps.Infrastructure.Persistence.Contexts;

namespace RetailOps.Infrastructure.Cutover;

public static class CutoverDatabaseBootstrap
{
    public static async Task EnsureSchemaAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RetailOpsDbContext>();
        await db.Database.MigrateAsync(ct);
    }
}
