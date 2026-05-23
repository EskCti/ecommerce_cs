using Microsoft.EntityFrameworkCore;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Platform.Core.Application.Ports;
using RetailOps.Platform.Core.Domain.Entities;
using RetailOps.Platform.Core.Application.UseCases;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Platform.Infrastructure.Legacy;

public sealed class LegacyCompanyRepository(LegacySasDbContext db) : ICompanyRepository
{
    public async Task<Company?> FindByIdAsync(int legacyCompanyId, CancellationToken ct = default)
    {
        var row = await db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == legacyCompanyId, ct);
        if (row is null) return null;

        var contracts = await db.Contracts.AsNoTracking()
            .Where(c => c.CompanyId == legacyCompanyId)
            .ToListAsync(ct);

        var mapped = LegacyCompanyMapper.ToDomain(row, contracts);
        return mapped.IsSuccess ? mapped.Value : null;
    }

    public async Task<Company?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var row = await db.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == normalized, ct);
        if (row is null) return null;

        return await FindByIdAsync(row.Id, ct);
    }

    public async Task<ResultPage<Company>> ListAsync(CompanyListFilter filter, CancellationToken ct = default)
    {
        var query = db.Companies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => c.Name.ToLower().Contains(filter.Name.Trim().ToLower()));

        if (filter.Active is true)
            query = query.Where(c => c.Active == "Sim");
        else if (filter.Active is false)
            query = query.Where(c => c.Active != "Sim");

        if (filter.Trial is true)
            query = query.Where(c => c.Trial == "Sim");
        else if (filter.Trial is false)
            query = query.Where(c => c.Trial != "Sim");

        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderBy(c => c.Name)
            .Skip(Math.Max(0, (filter.Page - 1) * filter.PageSize))
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var items = new List<Company>();
        foreach (var row in rows)
        {
            var mapped = LegacyCompanyMapper.ToDomain(row, []);
            if (mapped.IsSuccess)
                items.Add(mapped.Value);
        }

        return new ResultPage<Company>(items, total);
    }

    public async Task<Result<int>> SaveAsync(Company company, CancellationToken ct = default)
    {
        LegacyCompanyRow row;
        if (company.LegacyCompanyId <= 0)
        {
            row = new LegacyCompanyRow();
            db.Companies.Add(row);
        }
        else
        {
            row = await db.Companies.FirstOrDefaultAsync(c => c.Id == company.LegacyCompanyId, ct)
                ?? new LegacyCompanyRow();
            if (row.Id == 0)
                db.Companies.Add(row);
        }

        LegacyCompanyMapper.ApplyToRow(company, row);
        await db.SaveChangesAsync(ct);
        return Result<int>.Success(row.Id);
    }

    public async Task<Result> SaveContractAsync(int legacyCompanyId, Contract contract, CancellationToken ct = default)
    {
        LegacyContractRow row;
        if (contract.LegacyContractId <= 0)
        {
            row = new LegacyContractRow { CompanyId = legacyCompanyId };
            db.Contracts.Add(row);
        }
        else
        {
            row = await db.Contracts.FirstOrDefaultAsync(c => c.Id == contract.LegacyContractId, ct)
                ?? new LegacyContractRow { CompanyId = legacyCompanyId };
            if (row.Id == 0)
                db.Contracts.Add(row);
        }

        row.Text = contract.Text;
        row.SignedDate = contract.SignedDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        row.CompanyId = legacyCompanyId;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public sealed class LegacyPlatformConfigRepository(LegacySasDbContext db) : IPlatformConfigRepository
{
    public async Task<PlatformConfig?> GetGlobalAsync(CancellationToken ct = default)
    {
        try
        {
            var row = await db.Configs.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CompanyId == 0, ct);

            if (row is null)
                return PlatformConfig.Create(7, 5, "Conta suspensa.").Value;

            return PlatformConfig.Create(
                row.TrialDays ?? 7,
                row.BlockDays ?? 5,
                row.BlockMessage ?? "Conta suspensa.").Value;
        }
        catch
        {
            return PlatformConfig.Create(7, 5, "Conta suspensa.").Value;
        }
    }
}

public sealed class LegacyTenantInvoiceAdapter(LegacySasDbContext db) : ITenantInvoicePort
{
    public async Task<Result> IssueInvoiceAsync(int tenantId, decimal amount, DateOnly dueDate, CancellationToken ct = default)
    {
        db.Receivables.Add(new LegacyReceivableRow
        {
            Type = "Empresa",
            PersonId = tenantId,
            Amount = amount,
            DueDate = dueDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            Paid = "Não",
        });

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public sealed class InProcessTenantSuspendedPublisher(DeactivateUsersOnTenantSuspendedHandler handler)
    : ITenantSuspendedPublisher
{
    public async Task PublishAsync(int tenantId, CancellationToken ct = default)
    {
        await handler.HandleAsync(tenantId, ct);
    }
}
