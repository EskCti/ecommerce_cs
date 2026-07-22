# PHP Decommission Runbook (EP-011)

Operational checklist for cutover from legacy PHP (`exemplos/php/vendas`) to RetailOps C#/Vue.

## Prerequisites

- [ ] Pre-cutover gate green: `GET /api/admin/cutover-checklist` (platform tenant)
- [ ] Historical ETL completed: `dotnet run --project apps/backend/RetailOps.Migration`
- [ ] Reconciliation pass: `dotnet run --project apps/backend/RetailOps.Migration -- --reconcile --tenant=<id>`
- [ ] Full regression: `dotnet test` in `apps/backend/tests`
- [ ] MD5-only users reset (bcrypt-only login enforced)

## Phase 1 — Backup (required before ACL removal)

Run `scripts/backup-pre-cutover.sh` on the production host.

1. **Database**: `pg_dump -Fc sas_vendas > backups/sas-YYYYMMDD.dump`
2. **Uploads**: `tar -czf backups/uploads-YYYYMMDD.tar.gz /path/to/uploads`
3. Record SHA-256 checksums in the backup log
4. **Verify**: restore dump to staging and smoke-test login + one sale

## Phase 2 — Traffic cutover

1. Deploy API + Vue with production migration flags:
   - `Migration__LegacyPhpEnabled=false`
   - `Migration__AllTenantsOnRetailOps=true`
   - `Migration__DualWriteEnabled=false`
2. Confirm no PHP upstream in Nginx/YARP (this repo ships C#/Vue-only compose)
3. Monitor `GET /api/admin/migration-status` → `tenantsOnPhpPath: 0`

### Tenant waves (optional gradual rollout)

Expand `Migration__FullyMigratedTenantIds` in waves (25% → 50% → 100%) with 48h soak between waves before setting `AllTenantsOnRetailOps=true`.

## Phase 3 — ACL removal (blocked until normalized EF repos exist)

See `docs/cutover/acl-removal-prerequisites.md`. Do **not** drop `RetailOps.Infrastructure.Legacy` until normalized repositories replace every legacy adapter.

## Phase 4 — Post-cutover monitoring (30 days)

See `docs/monitoring/post-cutover-alerts.md`.

## Rollback (4-hour window)

1. Restore database from latest verified dump
2. Restore uploads tarball if needed
3. Re-enable PHP routing at edge gateway (if external)
4. Set `Migration__LegacyPhpEnabled=true` and redeploy previous API image
5. Communicate maintenance status to affected tenants

## Communication template

> Manutenção programada: migração do sistema de vendas para a nova plataforma RetailOps.
> Janela: [DATA/HORA]. Impacto esperado: indisponibilidade de até [N] minutos.
> Após a migração, acesse [URL Vue] com suas credenciais existentes.
