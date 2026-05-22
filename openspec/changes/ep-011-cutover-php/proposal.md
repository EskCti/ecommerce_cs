## Why

Após EP-001–EP-010, o Strangler Fig precisa **encerrar o legado PHP** com tráfego 100% na stack C#/Vue/Android, dados históricos em schema normalizado e remoção da ACL temporária — hoje acoplada a `sas.sql` e roteamento YARP. Sem cutover formal (migration-strategy **Fase 11**), o projeto permanece em dual-write indefinido, com risco operacional e dívida de infraestrutura.

## What Changes

- Job **ETL histórico** copiando tabelas legado → schema EF Core normalizado (Fase 7+ → 11)
- **Remoção rotas YARP/Nginx** para PHP; SPA Vue + API C# únicos entrypoints
- **Desligamento feature flags** `Migration:Modules` — todos tenants em C#; zero flag PHP
- **Remoção projetos `*.Infrastructure.Legacy`** e DbContext ACL após validação
- **Runbook decommission**: backup `sas.sql` + filesystem uploads; checklist operacional
- **Monitoramento pós-cutover** 30 dias (métricas, alertas, rollback plan documentado)
- **`test:e2e` regressão full suite** como gate de go-live
- Auth pós-cutover: bcrypt-only; desativar validação MD5 ACL

## Capabilities

### New Capabilities

- `historical-etl`: Migração batch histórico legado → tabelas normalizadas idempotente
- `php-traffic-cutover`: Remover proxy PHP; 100% tráfego API C# + Vue
- `legacy-acl-removal`: Remover adapters/mappers Legacy por BC após ETL validado
- `decommission-runbook`: Backup, checklist, rollback e comunicação operacional
- `cutover-validation`: Gates de aceitação Fase 11 + regressão E2E completa

### Modified Capabilities

- `multi-tenancy`: Eliminar roteamento pilot/legacy; todos tenants fully migrated
- `legacy-data-acl`: Deprecar e remover `LegacySasDbContext` e dual-write após cutover

## Impact

- **Infra**: YARP/Nginx config, Docker compose prod, feature flags appsettings
- **Todos BCs EP-001–010**: switch de ACL para repositórios EF normalizados
- **Identity**: MD5 login path removido; lazy bcrypt migration concluída ou reset forçado
- **Deploy**: PHP container/process removido de produção (repo mantém `exemplos/` histórico)
- **Dependências**: EP-001–EP-010 completos · **Release**: 4
