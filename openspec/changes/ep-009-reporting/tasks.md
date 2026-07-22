# Tasks — ep-009-reporting (EP-009 Reporting)

Referência: `docs/planning/loja-php/backlog.md` · US-090, US-091

## 1. Module setup

- [x] 1.1 `infra:setup` Módulo Reporting (~1h)
  - **Agent:** `Config New Module (C#)`
  - **Prompt:** "Crie RetailOps.Reporting.Application + Infrastructure; IPdfRendererPort, IReportingLegacyPort; sem agregados mutáveis."
  - **Spec:** `pdf-rendering`, `reporting-legacy-acl`

- [x] 1.2 `domain:vo` ReportFilter VOs (~1h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "DateRange, ReportFilter (dates, CustomerId?, SellerId?, Status?), ReportFormat enum PDF|HTML."
  - **Spec:** `report-filters`

## 2. Infrastructure — PDF & ACL

- [x] 2.1 `infra:adapter` QuestPdfRendererAdapter (~4h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "QuestPDF IPdfRendererPort; templates sales, low-stock, cash-sessions, profit, receipt; logo tenant."
  - **Spec:** `pdf-rendering`

- [x] 2.2 `infra:adapter` LegacyReportSqlAdapter (~3h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "LegacyReportSqlAdapter read-only SELECT; LegacyReportMapper rows→DTOs; paridade vendas_class, estoque_class, caixas_class, lucro_class, comprovante ACL."
  - **Spec:** `reporting-legacy-acl`

## 3. Application — US-090 operational queries

- [x] 3.1 `app:query` ReportSalesQuery (~3h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "ReportSalesQuery + SalesReportRow; filtros DateRange, customer, seller RF-082; tenant scope."
  - **Spec:** `operational-reports`, `report-filters`

- [x] 3.2 `app:query` ReportLowStockQuery (~2h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "ReportLowStockQuery + LowStockRow; produtos abaixo nível mínimo RF-080."
  - **Spec:** `operational-reports`

- [x] 3.3 `app:query` ReportCashSessionsQuery (~3h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "ReportCashSessionsQuery + CashSessionReportRow; período abertura/fechamento RF-080."
  - **Spec:** `operational-reports`

## 4. Application — US-091 receipt & profit

- [x] 4.1 `app:query` GenerateReceiptQuery (~3h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "GenerateReceiptQuery + ReceiptDocument; SaleId; store header, linhas, pagamento, troco RF-057."
  - **Spec:** `sale-receipt`, `sale-finalization`

- [x] 4.2 `app:query` ReportProfitQuery (~3h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "ReportProfitQuery + ProfitStatement; receitas − custos − despesas período; read Finance ACL."
  - **Spec:** `profit-statement`

## 5. API

- [x] 5.1 `interface:controller` ReportingController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "GET /api/reporting/sales|low-stock|cash-sessions|profit|receipts/{saleId}; PDF/HTML Content-Type; RBAC grupo Relatórios."
  - **Spec:** `operational-reports`, `profit-statement`, `sale-receipt`

## 6. Frontend Vue

- [x] 6.1 `interface:page` Reports hub (~3h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "/reports hub cards; modal filtros Calendar/Dropdown RF-082; blob download PDF."
  - **Spec:** `reporting-web-ui`

- [x] 6.2 `interface:page` PDV receipt action (~2h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "Botão imprimir recibo pós-venda PDV; chama GET /api/reporting/receipts/{saleId}."
  - **Spec:** `reporting-web-ui`, `sale-receipt`

## 7. Tests

- [x] 7.1 `test:e2e` Reporting PDF endpoints (~3h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "WebApplicationFactory: sales/low-stock/profit/receipt PDF retornam 200 + application/pdf; tenant isolation."

- [x] 7.2 `test:unit` ReportFilter validation (~1h)
  - **Agent:** `Unit Tests (C#)`
  - **Prompt:** "xUnit DateRange invalid from>to → 400; ReportFilter optional fields."

## 8. Acceptance verification

- [x] 8.1 Validar US-090: PDF vendas, estoque baixo, caixas com filtros período
- [x] 8.2 Validar US-091: comprovante pós-venda + demonstrativo lucro
- [x] 8.3 Comparar amostra PDF QuestPDF vs dompdf legado (valores e layout)
