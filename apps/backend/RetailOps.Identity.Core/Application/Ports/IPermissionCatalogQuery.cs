using RetailOps.Identity.Core.Application.Dtos;

namespace RetailOps.Identity.Core.Application.Ports;

public interface IPermissionCatalogQuery
{
    Task<IReadOnlyList<PermissionCatalogItemDto>> ListAsync(CancellationToken ct = default);
}
