# Tasks — ep-011-cutover-php (EP-011 Cutover PHP)

Referência: `docs/planning/loja-php/backlog.md` · US-110 · `migration-strategy.md` Fase 11

## 1. Pre-cutover validation

- [ ] 1.1 `ops:checklist` Gate checklist EP-001–010 (~2h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "Checklist automatizado: todos Migration Modules true; Sales parallel run ≤0.1% 2 semanas; critérios Fase 11 migration-strategy."
  - **Spec:** `cutover-validation`

## 2. Historical ETL

- [ ] 2.1 `infra:etl` HistoricalDataMigrationJob (~8h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "Console job idempotente: LegacySasDbContext → tabelas normalizadas por BC; MigrationCheckpoint; ordem Platform→Returns."
  - **Spec:** `historical-etl`

- [ ] 2.2 `infra:etl` Reconciliation report (~2h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "ReconcileReport: row counts + checksum amostral Sales/Catalog/Finance; falha bloqueia cutover."
  - **Spec:** `historical-etl`, `cutover-validation`

## 3. Traffic cutover

- [ ] 3.1 `infra:routing` Remover YARP routes PHP (~2h)
  - **Agent:** `Config Docker (C#)`
  - **Prompt:** "Remover clusters PHP yarp.json/Nginx; Vue SPA catch-all; LegacyPhpEnabled false appsettings prod."
  - **Spec:** `php-traffic-cutover`

- [ ] 3.2 `infra:routing` Tenant wave cutover (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "Expandir FullyMigratedTenantIds 25→50→100%; soak 48h por wave; GET /api/admin/migration-status."
  - **Spec:** `php-traffic-cutover`, `multi-tenancy`

## 4. ACL removal

- [ ] 4.1 `infra:cleanup` Remover Infrastructure.Legacy (~4h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "Deletar *.Infrastructure.Legacy por ordem acl-design; DI só EF repos; desabilitar SaleParallelRunLogger dual-write."
  - **Spec:** `legacy-acl-removal`, `legacy-data-acl`

- [ ] 4.2 `infra:cleanup` Auth MD5 path removal (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Remover LegacyMd5PasswordVerifier; login bcrypt-only; job pré-cutover reset users MD5-only."
  - **Spec:** `legacy-acl-removal`

## 5. Decommission runbook

- [ ] 5.1 `docs:runbook` PHP decommission (~4h)
  - **Agent:** `Config CI/CD (C#)`
  - **Prompt:** "docs/runbooks/php-decommission.md: pg_dump sas, tar uploads, offsite verify, rollback 4h, comunicação tenants."
  - **Spec:** `decommission-runbook`

- [ ] 5.2 `ops:backup` Executar backup pré-cutover (~2h)
  - **Agent:** `Config Docker (C#)`
  - **Prompt:** "Executar backup sas.sql + uploads; restore test staging; registrar checksums."
  - **Spec:** `decommission-runbook`

## 6. Tests & monitoring

- [ ] 6.1 `test:e2e` Full regression suite (~4h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "Rodar suite completa Auth→Notifications; gate obrigatório antes prod cutover."
  - **Spec:** `cutover-validation`

- [ ] 6.2 `ops:monitor` Dashboard 30 dias (~2h)
  - **Agent:** `Config CI/CD (C#)`
  - **Prompt:** "Alertas 5xx, p95, sales daily vs baseline, stock discrepancy zero; rollback trigger documentado."
  - **Spec:** `decommission-runbook`

## 7. Acceptance verification

- [ ] 7.1 Validar US-110: zero tenants flag PHP
- [ ] 7.2 Validar ACL Legacy removida da solution
- [ ] 7.3 Validar backup sas.sql + uploads verificado
- [ ] 7.4 Validar 30 dias monitoramento sem rollback
