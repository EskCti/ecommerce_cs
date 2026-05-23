using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;

namespace RetailOps.Infrastructure.Legacy.Persistence;

public static class LegacyDatabaseBootstrap
{
    public static async Task EnsureDevSchemaAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LegacySasDbContext>();

        await db.Database.EnsureCreatedAsync(ct);
        await WidenLegacyColumnsAsync(db, ct);
        await SeedGlobalConfigAsync(db, ct);
    }

    private static async Task WidenLegacyColumnsAsync(LegacySasDbContext db, CancellationToken ct)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE IF EXISTS usuarios
              ALTER COLUMN senha_crip TYPE varchar(255),
              ALTER COLUMN senha TYPE varchar(255);
            """,
            ct);
    }

    private static async Task SeedGlobalConfigAsync(LegacySasDbContext db, CancellationToken ct)
    {
        if (await db.Configs.AnyAsync(c => c.CompanyId == 0, ct))
            return;

        db.Configs.Add(new LegacyConfigRow
        {
            CompanyId = 0,
            TrialDays = 7,
            BlockDays = 5,
            BlockMessage = "Conta suspensa. Entre em contato com o suporte.",
        });

        await db.SaveChangesAsync(ct);
    }
}
