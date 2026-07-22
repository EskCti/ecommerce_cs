# ACL Removal Prerequisites

EP-011 task 4.1 (remove `RetailOps.Infrastructure.Legacy`) is **blocked** until:

1. Normalized EF Core entities and migrations exist per bounded context in `RetailOps.Infrastructure`
2. Repository adapters switch from legacy mappers to normalized `RetailOpsDbContext`
3. Historical ETL and reconciliation reports pass for all production tenants
4. Traffic cutover complete (`LegacyPhpEnabled=false`, `AllTenantsOnRetailOps=true`)

Current state: the application reads and writes the legacy `sas` schema through ACL adapters. Removing Legacy infrastructure without normalized repositories would break all BC modules.

Removal order (from `acl-design.md`):

1. Notifications, Reporting (read-only)
2. Returns, CRM, Settings
3. Finance
4. Sales (after 2-week parallel run green)
5. Catalog
6. Identity
7. Platform

Track progress via `GET /api/admin/migration-status` and cutover checklist.
