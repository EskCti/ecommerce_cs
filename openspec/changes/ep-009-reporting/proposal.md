## Why

Gestores e operadores dependem de **relatórios PDF** do legado (`rel_sistema/`, dompdf) para acompanhar vendas, estoque, caixas e lucro — hoje com SQL inline e filtros em modais (`RF-080`, `RF-082`, `RF-057`). Após Finance (EP-007) e demais BCs operacionais, o **BC Reporting** (Generic, read-only CQRS) consolida projeções e exportação PDF via QuestPDF, sem replicar anti-patterns do PHP.

## What Changes

- Novo módulo `RetailOps.Reporting` (Application + Infrastructure — sem agregados mutáveis)
- VOs compartilhados: `DateRange`, `ReportFilter`, `ReportFormat`
- Queries CQRS tenant-scoped: `ReportSalesQuery`, `ReportLowStockQuery`, `ReportCashSessionsQuery`, `ReportProfitQuery`, `GenerateReceiptQuery`
- Port `IPdfRendererPort` + `QuestPdfRendererAdapter` (substitui dompdf)
- ACL read-only: `LegacyReportSqlAdapter`, `LegacyReportMapper` para paridade Fase 9
- API: endpoints GET que retornam PDF (ou HTML quando `ReportFormat` = HTML)
- Frontend Vue: hub de relatórios com filtros por período/cliente/vendedor/status RF-082
- Comprovante pós-venda integrado ao PDV RF-057
- Testes E2E geração PDF + comparação amostral vs legado

## Capabilities

### New Capabilities

- `operational-reports`: Relatórios vendas, estoque baixo e sessões de caixa PDF RF-080
- `profit-statement`: Demonstrativo de lucro simplificado (receitas − custos/despesas) RF-080
- `sale-receipt`: Comprovante/recibo de venda PDF RF-057
- `report-filters`: Filtros date range, status, cliente, vendedor RF-082
- `pdf-rendering`: Abstração QuestPDF para exportação RNF-008
- `reporting-legacy-acl`: SQL read-only legado → read models CQRS
- `reporting-web-ui`: UI Vue hub relatórios + download PDF

### Modified Capabilities

- `store-config`: `ReportFormat` (PDF/HTML) da loja determina content-type de saída dos relatórios tenant
- `sale-finalization`: Finalização de venda expõe ação/link para `GenerateReceiptQuery` RF-057

## Impact

- **Backend**: módulo Reporting read-side; depende de dados Sales, Catalog, Finance, Returns via ACL/queries
- **Store Settings EP-003**: `ReportFormat` e logo relatório consumidos na renderização
- **Sales EP-006**: comprovante pós-`SaleCompleted`
- **Finance EP-007**: lucro e receber/pagar alimentam `ReportProfitQuery`
- **Platform EP-002**: relatórios SAS (`RF-081`) fora do escopo MVP tenant — stub futuro
- **Dependências**: EP-000, EP-001, EP-007 · **Release**: 3
