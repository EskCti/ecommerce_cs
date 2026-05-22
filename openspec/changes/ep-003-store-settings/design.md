## Context

**RetailOps** reimplementa PDV SaaS multi-tenant. EP-003 implementa **BC Store Settings** (Supporting) — configuração operacional por tenant, distinta da config global SAS (Platform EP-002).

Legado:
- `config` where `empresa = tenant_id` — RF-027
- `forma_pgtos` — RF-025, RN-047
- `caixas` (cadastro terminal, não sessão) — RF-026

Sessões de caixa (`caixa` aberto/fechado) pertencem a **Sales BC (EP-006)**; este épico cobre apenas **terminais físicos** (`caixas` table).

Referências: `ddd-tactical-model.md` (BC Store Settings), `acl-design.md` (BC Store Settings section).

## Goals / Non-Goals

**Goals:**

- Paridade RF-027, RF-025, RF-026 para administrador tenant
- Um `StoreConfig` por tenant; CRUD PaymentMethod e CashRegisterTerminal
- `PaymentSurchargeCalculator` exposto para Sales BC calcular acréscimo
- ACL `LegacyStoreConfigMapper`, `LegacyPaymentMethodMapper`, `LegacyCashRegisterTerminalMapper`
- UI Vue com PrimeVue forms e DataTables
- Autorização: administrador tenant + permissões granulares EP-001

**Non-Goals:**

- Abertura/fechamento de sessão de caixa (EP-006 Sales)
- Upload de logo para object storage avançado (MVP: path/URL string compatível `foto_rel`)
- Integração WhatsApp API (apenas persistir token)
- Gestão de cargos/usuários (Identity EP-001 / legado RF-024)

## Decisions

### 1. Módulo StoreSettings

**Decisão:** `Config New Module (C#)` → `RetailOps.StoreSettings.*`; tenant filter em todos repositórios via `ITenantContext`.

### 2. StoreConfig aggregate

**Decisão:** Singleton per tenant; métodos `UpdateGeneralInfo()`, `UpdateDiscountSettings()`, `UpdateReportSettings()`, `UpdateIntegrationToken()`; create-on-first-read se ausente no legado.

**Campos mapeados:** nome_sistema, contatos, cnpj_sistema, endereço, tipo_rel, tipo_desconto, comissao, token, foto_rel.

### 3. PaymentMethod aggregate

**Decisão:** `PaymentMethod` AR com `Name`, `SurchargePercent` (0–100); validar nome único por tenant.

**RN-047:** `PaymentSurchargeCalculator.Calculate(baseAmount, methodId)` retorna valor com acréscimo — exposto como domain service interface consumível por Sales.

### 4. CashRegisterTerminal aggregate

**Decisão:** `CashRegisterTerminal` AR: `Name`, `Status` (Open/Closed enum para cadastro terminal legado), `AssignedOperatorId` (UserId ref Identity, nullable).

**Nota:** status no cadastro `caixas` reflete terminal; sessão PDV usa tabela `caixa` em EP-006.

### 5. API design

**Decisão:**
- `GET/PUT /api/settings/store-config`
- `GET/POST/PUT/DELETE /api/settings/payment-methods`
- `GET/POST/PUT/DELETE /api/settings/cash-registers`

Todos exigem JWT tenant scope + permission keys mapeadas do legado `acessos`.

### 6. Frontend

**Decisão:** Rotas `/settings/store`, `/settings/payment-methods`, `/settings/cash-registers`; forms vee-validate; listagens DataTable.

### 7. Parallel run

**Decisão:** Writes via `IStoreSettingsLegacyPort` durante migração; EF schema `store_settings_*` opcional em paralelo ou ACL-only no MVP.

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| Confusão caixas vs caixa sessão | Documentação + nomes `CashRegisterTerminal` vs `CashSession` |
| Logo upload | MVP string path; file upload hook EP posterior |
| Operator link invalid user | Validar UserId exists via Identity query port |
| Duplicate payment method names | Unique constraint per tenant no domain |

## Migration Plan

1. Implementar módulo inside-out
2. Pilot tenant flag roteia settings para RetailOps API
3. EP-006 PDV referencia PaymentMethod/CashRegisterTerminal IDs

**Rollback:** pilot flag off; PHP `editar-config.php` / CRUD legado.

## Open Questions

- File upload logo para S3/local storage? **MVP: URL/path manual**
- Permissão exata por menu legado? **Mapear chaves `configuracoes.*` no seed EP-001**
