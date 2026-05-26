namespace RetailOps.Core.Crm.Application.Events;

public sealed record CustomerRegistered(Guid CustomerId, int TenantId, string Cpf);

public sealed record CustomerFoundByCpf(Guid CustomerId, int TenantId, string Cpf);
