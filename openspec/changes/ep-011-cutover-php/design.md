## Context

**RetailOps** — épico **TECH Enabler** EP-011. Release 4; depende de todos BCs EP-001–EP-010 operacionais com parallel run validado (especialmente Sales ≤0,1% divergência). Legado: Strangler Fig desde EP-000 com YARP, feature flags e `Infrastructure.Legacy`.

Referências: `migration-strategy.md` Fase 11, `acl-design.md` cronograma remoção ACL, `backlog.md` US-110.

## Goals / Non-Goals

**Goals:**

- US-110: 100% tráfego C# sem PHP em produção
- Zero tenants com flag PHP / legacy routing
- ETL histórico completo e validado
- Backup `sas.sql` + uploads antes de drop ACL
- 30 dias monitoramento sem rollback
- Full E2E regression gate
- Runbook operacional documentado

**Non-Goals:**

- Reimplementar funcionalidades faltantes (deve estar done em EP-001–010)
- Migração para microserviços ou multi-region
- Remover `exemplos/php/vendas` do repositório git (manter histórico)
- E-commerce B2C novo (fora de escopo estratégico)

## Decisions

### 1. Cutover phases (within EP-011)

```
Phase A — Pre-cutover validation (gate)
Phase B — ETL histórico + reconcile
Phase C — Tenant wave cutover (flags → all C#)
Phase D — Remove YARP PHP routes
Phase E — ACL removal + schema cleanup
Phase F — 30-day monitoring
```

### 2. ETL histórico

**Decisão:** Console job `HistoricalDataMigrationJob` idempotente:

- Source: tabelas legado via `LegacySasDbContext` (read)
- Target: tabelas normalizadas EF por BC
- Ordem: Platform → Identity → Settings → Catalog → CRM → Sales → Finance → Returns
- Checkpoint table `MigrationCheckpoint` por tenant+BC
- Reconcile report: row counts + checksum samples vs legado

**Alternativa rejeitada:** Big bang sem reconcile — alto risco Finance/Sales.

### 3. Tenant cutover waves

**Decisão:** Expandir `Migration:PilotTenantIds` → `Migration:FullyMigratedTenantIds` até 100%:

1. Piloto final validation
2. Wave 25% → 50% → 100% tenants produção
3. Cada wave: 48h soak + parallel run metrics green

Config final:

```json
{
  "Migration": {
    "LegacyPhpEnabled": false,
    "AllTenantsOnRetailOps": true
  }
}
```

### 4. YARP / routing removal

**Decisão:** Remover clusters/routes PHP de `yarp.json` / Nginx upstream `php-fpm`.

| Before | After |
| ------ | ----- |
| `/vendas/*` → PHP | 404 or redirect to Vue SPA |
| `/api/*` → C# | unchanged |
| Vue SPA catch-all | `/` → `web-vue` |

PHP container removed from `docker-compose.prod.yml`.

### 5. ACL removal order

Per `acl-design.md`:

| Order | BC | Condition |
| ----- | -- | --------- |
| 1 | Notifications, Reporting | HTTP/SQL read-only — first |
| 2 | Returns, CRM, Settings | Medium ACL |
| 3 | Finance | Receivable/payable normalized |
| 4 | Sales | Parallel run OK 2 weeks |
| 5 | Catalog | Stock reconciled |
| 6 | Identity | 100% bcrypt |
| 7 | Platform | `platform_*` schema owns SAS |

**Action:** Delete `RetailOps.*.Infrastructure.Legacy` projects; update DI to EF repositories only.

### 6. Auth post-cutover

**Decisão:** Remove `LegacyMd5PasswordVerifier`; login rejects non-bcrypt hashes with forced password reset flow.

Job pré-cutover: identify users still MD5-only → email reset or admin bulk reset.

### 7. Backup & runbook

**Decisão:** `docs/runbooks/php-decommission.md`:

- pg_dump `sas` full
- tar uploads directory
- S3/offsite copy verification
- Rollback: restore dump + re-enable YARP PHP (timeboxed 4h procedure)

### 8. Monitoring 30 days

**Decisão:** Dashboard/alerts:

- API 5xx rate, p95 latency
- Sales count daily vs baseline
- Stock discrepancy job (should be zero)
- Failed digest/report jobs
- **Rollback trigger:** critical business metric deviation >1% for 24h

## Risks / Trade-offs

| Risk | Mitigation |
| ---- | ---------- |
| Data loss on ETL | Idempotent job + reconcile + backup before drop |
| Missed edge case in ACL | Full E2E suite + 30-day monitoring |
| Rollback needed after ACL drop | Backup restore runbook; delay ACL drop 7 days after traffic cutover |
| MD5 users locked out | Pre-cutover reset campaign + support window |

## Migration Plan

1. Execute pre-cutover checklist (all EP acceptance criteria)
2. Run ETL + reconcile report — sign-off required
3. Disable PHP routing per environment (staging → prod)
4. Set `LegacyPhpEnabled: false` globally
5. Monitor 7 days with ACL still present (fast rollback)
6. Remove Legacy infrastructure projects
7. Monitor 30 days total; archive backup

## Open Questions

- Drop legado tables immediately or keep read-only views 90 days?
- Tenant communication template for maintenance window?
