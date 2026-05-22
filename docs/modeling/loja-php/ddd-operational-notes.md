# Notas Operacionais — Loja PHP (Sistema de Vendas SaaS)

**Data**: 2026-05-22

---

## Topologia Recomendada

**Monólito modular** com separação lógica de apps (espelhando legado `sas/` vs `sistema/`).

| Critério | Avaliação | Decisão |
| -------- | --------- | ------- |
| Equipe inicial | Pequena; domínio recém-modelado | Monólito |
| Deploy | Cadência única aceitável na v1 | Monólito |
| Carga | PDV + backoffice similares | Sem split por escala |
| Maturidade | Migração de legado PHP | Explorar em monólito |
| Multi-tenant | Filtro TenantId transversal | Middleware + módulos |

**Evolução futura**: extrair **Platform** ou **Notifications** para serviço separado apenas se deploy/escala independente for necessário.

```
┌─────────────────────────────────────────────────────────┐
│              MONÓLITO MODULAR (recomendado)              │
├─────────────────────────────────────────────────────────┤
│  apps/                                                   │
│    backend/          ← API REST (NestJS / Spring / .NET) │
│    web-platform/     ← painel SAS (opcional app separado)│
│    web-tenant/       ← painel loja                       │
│  packages/                                               │
│    platform/         ← BC Platform                       │
│    auth/             ← BC Identity & Access             │
│    settings/         ← BC Store Settings                │
│    catalog/          ← BC Catalog & Inventory             │
│    sales/            ← BC Sales                         │
│    returns/          ← BC Returns                         │
│    finance/          ← BC Finance                         │
│    crm/              ← BC CRM                             │
│    reporting/        ← BC Reporting (queries)           │
│    notifications/    ← BC Notifications                   │
│    shared/           ← TenantId, Money, Result, Entity    │
└─────────────────────────────────────────────────────────┘
```

---

## Mapeamento BC → Módulo → Skill

Stack a definir no planejamento (`req-agile-planning`). Skills abaixo listam variantes TS / KT / CS.

| BC | Módulo package | App backend module | Skill scaffold | Prioridade |
| -- | -------------- | ------------------ | -------------- | ---------- |
| shared | `packages/shared` | — | `config-shared-core` / `-kt` / `-cs` | P0 |
| Platform | `packages/platform` | `modules/platform` | `config-new-module` / `-kt` / `-cs` | P1 |
| Identity & Access | `packages/auth` | `modules/auth` | `config-auth-core-full` + `config-auth-backend-basic` | P0 |
| Store Settings | `packages/settings` | `modules/settings` | `config-new-module` | P2 |
| Catalog & Inventory | `packages/catalog` | `modules/catalog` | `config-new-module` | P1 |
| Sales (PDV) | `packages/sales` | `modules/sales` | `config-new-module` | P1 |
| CRM | `packages/crm` | `modules/crm` | `config-new-module` | P2 |
| Finance | `packages/finance` | `modules/finance` | `config-new-module` | P2 |
| Returns | `packages/returns` | `modules/returns` | `config-new-module` | P3 |
| Reporting | `packages/reporting` | `modules/reporting` | `core-query-cqrs` + adapter PDF | P3 |
| Notifications | `packages/notifications` | `modules/notifications` | `config-new-module` | P4 |

### Mapeamento artefato → skill (por camada)

| Artefato | TS | KT | CS |
| -------- | -- | -- | -- |
| Value Object | `core-value-object` | `core-value-object-kt` | `core-value-object-cs` |
| Entity / Aggregate | `core-entity` | `core-entity-kt` | `core-entity-cs` |
| Domain Service | `core-domain-service` | `core-domain-service-kt` | `core-domain-service-cs` |
| Repository | `core-repository` | `core-repository-kt` | `core-repository-cs` |
| Use Case | `core-use-case` | `core-use-case-kt` | `core-use-case-cs` |
| DTO | `core-dto` | `core-dto-kt` | `core-dto-cs` |
| Query CQRS | `core-query-cqrs` | `core-query-cqrs-kt` | `core-query-cqrs-cs` |
| Persistence | `backend-prisma-data` | `backend-data-kt` | `backend-data-cs` |
| Controller | `backend-controller` | `backend-controller-kt` | `backend-controller-cs` |
| Frontend tenant | `frontend-page` + `frontend-form-schema` | — | — |

---

## Prioridade de Implementação

Ordem baseada em subdomínios **Core** e dependências do context map.

### Fase 0 — Bootstrap (EP-000)

- `config-project-fullstack` (stack TBD)
- `config-docker` + `config-cicd`
- `config-shared-core` + auth básico
- TenantId middleware, multi-tenancy

### Fase 1 — Fundação (P0–P1)

| Ordem | BC | Justificativa |
| ----- | -- | ------------- |
| 1 | Identity & Access | Prerequisite de todo fluxo |
| 2 | Platform | Trial, tenants, SAS admin |
| 3 | Catalog & Inventory | Produtos e estoque antes do PDV |
| 4 | Sales (PDV) | Core value — fluxo principal |

**Critério de done fase 1**: trial → login → cadastrar produto → abrir caixa → vender → fechar caixa → recibo.

### Fase 2 — Operação completa (P2)

| Ordem | BC | Justificativa |
| ----- | -- | ------------- |
| 5 | Store Settings | Formas pagamento, config loja, caixas |
| 6 | CRM | Clientes/fornecedores para fiado e compras |
| 7 | Finance | Contas, comissões, fluxo; integração SaleCompleted |

**Critério de done fase 2**: compra mercadoria, fiado, baixa contas, comissões.

### Fase 3 — Complementos (P3–P4)

| Ordem | BC | Justificativa |
| ----- | -- | ------------- |
| 8 | Returns | Trocas |
| 9 | Reporting | Paridade relatórios legado |
| 10 | Notifications | WhatsApp digest |

---

## ACL e integrações na implementação

| Fronteira | Tipo | Implementação sugerida |
| --------- | ---- | ---------------------- |
| Sales → Catalog | OHS | `ProductCatalogPort.findByBarcode()` |
| Sales → Finance | Published Language + handler | `SaleCompletedHandler` cria aggregates Finance |
| Returns → Catalog | Command | `StockAdjustmentPort.adjust()` |
| Platform → Finance | Event | `TenantInvoiceIssued` |
| Notifications → * | ACL + queries | `DigestQueryService` agrega read models |
| WhatsApp externo | ACL | `WhatsAppGatewayAdapter` |

---

## Migração do legado

Recomendado usar `req-migration-strategy` com padrão **Strangler Fig**:

1. Conviver legado PHP + novo backend por tenant piloto
2. Migrar auth e tenants primeiro
3. PDV como último módulo crítico (maior risco operacional)

---

## Decisões técnicas explícitas (não replicar legado)

| Legado | Nova implementação |
| ------ | ------------------ |
| MD5 senha | bcrypt/argon2 via PasswordHash VO |
| Senha gerente plaintext | ManagerPin separado ou MFA |
| `receber` polimórfica | Aggregates tipados: SaleReceivable, TenantInvoice, ManualReceivable |
| `itens_venda.venda=0` | CashSession aggregate com Cart entity |
| SQL concatenado | ORM + parameterized queries |
| Sessão + localStorage | JWT/httpOnly cookie ou session server-side |
| dompdf inline | Reporting module + PdfRendererPort |

---

## Referências de entrada

| Documento | Caminho |
| --------- | ------- |
| Requisitos | `docs/discovery/ecommerce-legado-php/requirements.md` |
| Discovery DDD | `docs/discovery/ecommerce-legado-php/ddd-analysis.md` |
| Modelo de domínio | `docs/discovery/ecommerce-legado-php/domain-model.md` |
| Modelo estratégico | `docs/modeling/loja-php/ddd-strategic-model.md` |
| Modelo tático | `docs/modeling/loja-php/ddd-tactical-model.md` |
