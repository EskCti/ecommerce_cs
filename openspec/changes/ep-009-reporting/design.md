## Context

**RetailOps** — BC **Reporting** (Generic, read side). Release 3; depende de EP-007 Finance e BCs operacionais com dados persistidos. Legado: `rel_sistema/*_class.php` + dompdf; `acl-design.md` mapeia scripts → queries CQRS.

**Princípio:** Reporting **não muta** domínio — apenas queries + renderização. Conformist consumer de Sales, Catalog, Finance, Returns, Store Settings.

Referências: `ddd-tactical-model.md` (BC Reporting), `acl-design.md` (BC Reporting section), `migration-strategy.md` Fase 9.

## Goals / Non-Goals

**Goals:**

- Paridade US-090: vendas, estoque baixo, caixas PDF com filtros RF-080/082
- Paridade US-091: comprovante venda RF-057 + demonstrativo lucro
- QuestPDF adapter substituindo dompdf (RNF-008)
- Tenant-scoped; logo e `ReportFormat` de Store Settings
- ACL read-only temporária para parallel run

**Non-Goals:**

- Relatórios SAS Platform (`RF-081` — empresas, contrato) — EP-002 futuro
- BI avançado, dashboards interativos, Excel export
- Todos os 14 relatórios legado no MVP — priorizar backlog US-090/091; demais queries stubbed
- WhatsApp digest (EP-010)

## Decisions

### 1. Módulo Reporting (read-only)

**Decisão:** `RetailOps.Reporting.Application` (queries + DTOs) + `RetailOps.Reporting.Infrastructure` (ACL + PDF). **Sem** `Reporting.Core` com agregados — apenas VOs em Application ou Shared.

### 2. CQRS queries

**Decisão:** Uma query handler por relatório; retorna read model + metadados; controller delega renderização.

| Query | Legado | Read model |
| ----- | ------ | ---------- |
| `ReportSalesQuery` | `vendas_class.php` | `SalesReportRow[]` |
| `ReportLowStockQuery` | `estoque_class.php` | `LowStockRow[]` |
| `ReportCashSessionsQuery` | `caixas_class.php` | `CashSessionReportRow[]` |
| `ReportProfitQuery` | `lucro_class.php` | `ProfitStatement` |
| `GenerateReceiptQuery` | `comprovante.php` | `ReceiptDocument` |

**Filtros:** `ReportFilter` VO composto — `DateRange`, optional `CustomerId`, `SellerId`, `PaymentStatus`.

### 3. PDF rendering

**Decisão:** `IPdfRendererPort` interface Application; `QuestPdfRendererAdapter` Infrastructure.

```csharp
Task<byte[]> RenderAsync<TModel>(string templateId, TModel model, ReportFormat format);
```

- `ReportFormat.Pdf` → `application/pdf`
- `ReportFormat.Html` → HTML inline (legacy `tipo_rel` HTML) — MVP: PDF primary, HTML stub acceptable

Templates QuestPDF por relatório; logo tenant de `IStoreConfigQueryPort`.

### 4. ACL read-only

**Decisão:** `IReportingLegacyPort` + `LegacyReportSqlAdapter` executa SQL parametrizado (somente SELECT) durante parallel run.

**Migration path:** substituir SQL legado por queries EF/Dapper sobre schema normalizado conforme BCs maduros.

**Domain isolation:** Application layer não referencia nomes `rel_sistema` ou colunas legado.

### 5. API routes

- `GET /api/reporting/sales?from&to&customerId&sellerId` → PDF
- `GET /api/reporting/low-stock` → PDF
- `GET /api/reporting/cash-sessions?from&to` → PDF
- `GET /api/reporting/profit?from&to` → PDF
- `GET /api/reporting/receipts/{saleId}` → PDF (RF-057)

Query params validados; tenant middleware obrigatório; RBAC grupo Relatórios (6).

### 6. Frontend Vue

**Decisão:** `/reports` hub com cards por relatório; modal filtros (PrimeVue Calendar, Dropdown cliente/vendedor); download via blob response.

PDV: botão "Imprimir recibo" pós-venda chama receipt endpoint.

### 7. Receipt integration

**Decisão:** `GenerateReceiptQuery` lê `SaleId` + linhas + pagamento + loja; não duplica lógica PDV — conformist read de Sales ACL.

## Risks / Trade-offs

| Risk | Mitigation |
| ---- | ---------- |
| PDF visual difere dompdf | Comparar amostras legado vs QuestPDF; templates iterativos |
| SQL legado lento | Índices tenant+data; paginação em listagens grandes |
| Escopo RF-080 completo | MVP US-090/091; demais relatórios em change futuro |
| HTML tipo_rel | Stub HTML; PDF default |

## Migration Plan

1. Deploy Reporting module com ACL read-only apontando mesmo DB legado
2. Parallel run: gerar PDF C# e PHP para mesma amostra; diff valores
3. Cutover: UI Vue substitui links `rel_sistema/*`; PHP reports desligados EP-011

## Open Questions

- Incluir relatórios receber/pagar/comissões/trocas no mesmo épico ou change follow-up?
- Template receipt: formato térmico 80mm vs A4 — legado usa qual?
