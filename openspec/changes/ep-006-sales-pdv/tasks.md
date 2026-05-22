# Tasks — ep-006-sales-pdv (EP-006 Sales PDV)

Referência: `docs/planning/loja-php/backlog.md` · US-060, US-061, US-062, US-063, US-064

## 1. Module setup

- [ ] 1.1 `infra:setup` Módulo Sales (~1h)
  - **Agent:** `Config New Module (C#)`
  - **Prompt:** "Crie RetailOps.Sales: Core, Application, Infrastructure; ICashSessionRepository, ISaleRepository; ports Catalog, CRM, Identity."
  - **Spec:** `cash-session-management`

## 2. Domain — CashSession (US-060, US-063)

- [ ] 2.1 `domain:vo` Sales VOs (~2h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "CashSessionId, SaleId, Discount, ChangeAmount, CashBreakage, ScanQuantityPrefix, SaleLineTotal, WarrantyDays."
  - **Spec:** `cash-session-management`, `sale-finalization`

- [ ] 2.2 `domain:entity` CashSession, SaleLine, CashWithdrawal (~3h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "CashSession AR: Open, AddLine, RemoveLine, RegisterWithdrawal, Close. SaleLine cart entity. CashWithdrawal."
  - **Spec:** `cash-session-management`, `pdv-cart`

- [ ] 2.3 `domain:service` CashSessionClosingPolicy (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Quebra RN-040: contado - (abertura + vendido - sangrias)."
  - **Spec:** `cash-session-management`

## 3. Domain — Sale (US-062, US-064)

- [ ] 3.1 `domain:entity` Sale aggregate (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Sale AR pós-finalize; PaymentTerms; Cancel() RN-060."
  - **Spec:** `sale-finalization`, `sale-cancellation`

- [ ] 3.2 `domain:service` SaleFinalizationPolicy, CommissionCalculator (~3h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "RN-041 fiado+cliente; RN-043 troco≥0; RN-044 carrinho vazio; RN-046 comissão."
  - **Spec:** `sale-finalization`

- [ ] 3.3 `domain:service` CartStockReservationService (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Orquestra IProductCatalogService Reserve/Release/Commit por linha carrinho."
  - **Spec:** `pdv-cart`, `catalog-open-host-service`

## 4. Application — Cash session (US-060, US-063)

- [ ] 4.1 `app:usecase` OpenCashSessionUseCase (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Valida terminal Settings, PIN gerente EP-001, fundo inicial; publica CashSessionOpened."
  - **Spec:** `cash-session-management`, `user-authentication`

- [ ] 4.2 `app:usecase` RegisterCashWithdrawal, CloseCashSession (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Sangria RF-055; CloseCashSession RF-056 com PIN gerente e quebra."
  - **Spec:** `cash-session-management`

## 5. Application — Cart (US-061)

- [ ] 5.1 `app:usecase` AddItemToCartUseCase (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "FindByBarcode Catalog; ScanQuantityPrefix 2*; RN-045 estoque; AddItemToCart."
  - **Spec:** `pdv-cart`

- [ ] 5.2 `app:usecase` ConfirmGradeForItemUseCase (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "ConfirmGradeForItem RF-052; reserva estoque grade."
  - **Spec:** `pdv-cart`

- [ ] 5.3 `app:usecase` RemoveCartLineUseCase (~1h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Remove line; release stock reservation."
  - **Spec:** `pdv-cart`

## 6. Application — Finalize & Cancel (US-062, US-064)

- [ ] 6.1 `app:usecase` FinalizeSaleUseCase (~4h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Finalize via LegacySaleAdapter; SaleCompleted event Finance stub; idempotency key."
  - **Spec:** `sale-finalization`, `sales-legacy-acl`

- [ ] 6.2 `app:usecase` CancelSaleUseCase (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "CancelSale RF-073 RN-060; SaleCancelled; restore stock Catalog."
  - **Spec:** `sale-cancellation`

- [ ] 6.3 `app:query` ListSalesQuery (~1h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "ListSalesQuery tipo Venda paginada filtros data, operador."
  - **Spec:** `sale-cancellation`

## 7. Infrastructure — ACL crítica (US-062)

- [ ] 7.1 `infra:persistence` LegacySaleAdapter (~6h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "LegacyCashSessionMapper, LegacyCartItemMapper, LegacySaleMapper, LegacyCommissionMapper, ISalesLegacyPort; transação única acl-design.md."
  - **Spec:** `sales-legacy-acl`

- [ ] 7.2 `infra:persistence` SaleParallelRunLogger (~2h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "SaleParallelRunLogger compara totais C# vs shadow PHP; alert >0.1%."
  - **Spec:** `sale-finalization`, `sales-legacy-acl`

## 8. API

- [ ] 8.1 `interface:controller` SalesCashSessionController (~3h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "open, current cart, add/remove line, confirm grade, withdrawal, close endpoints."
  - **Spec:** `cash-session-management`, `pdv-cart`

- [ ] 8.2 `interface:controller` SalesCartController + SalesController (~3h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "finalize; list sales; cancel sale."
  - **Spec:** `sale-finalization`, `sale-cancellation`

## 9. Frontend Vue PDV (US-060–063)

- [ ] 9.1 `interface:page` PDV workspace Vue (~6h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "Rota /pdv fullscreen: barcode input, cart lines, totals, open session modal."
  - **Spec:** `pdv-web-ui`

- [ ] 9.2 `interface:form-web` Checkout + cash management (~4h)
  - **Agent:** `Frontend Form (Vue)`
  - **Prompt:** "Checkout pagamento/desconto/cliente fiado/troco; sangria; fechamento quebra."
  - **Spec:** `pdv-web-ui`

- [ ] 9.3 `interface:page` Listagem vendas + cancel (~3h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "/sales list DataTable RF-073; ação cancelar venda gerente."
  - **Spec:** `sale-cancellation`, `pdv-web-ui`

## 10. Mobile Android PDV (US-060–062)

- [ ] 10.1 `interface:mobile` OpenCashSessionScreen (~4h)
  - **Agent:** `Mobile Screen (Android)`
  - **Prompt:** "Tela abertura caixa fundo + PIN gerente; ViewModel StateFlow."
  - **Spec:** `pdv-mobile`

- [ ] 10.2 `interface:mobile` PdvScanScreen (~6h)
  - **Agent:** `Mobile Screen (Android)`
  - **Prompt:** "Barcode scan camera; cart LazyColumn; grade dialog."
  - **Spec:** `pdv-mobile`, `pdv-cart`

- [ ] 10.3 `interface:mobile-form` Mobile checkout finalize (~4h)
  - **Agent:** `Mobile Form (Android)`
  - **Prompt:** "Checkout flow pagamento; chama finalize API."
  - **Spec:** `pdv-mobile`, `sale-finalization`

## 11. Tests

- [ ] 11.1 `test:unit` Sales domain + policies (~4h)
  - **Agent:** `Unit Tests (C#)`
  - **Prompt:** "xUnit: SaleFinalizationPolicy, CommissionCalculator, CashSessionClosingPolicy. Coverlet ≥95%."

- [ ] 11.2 `test:e2e` PDV fluxo completo (~4h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "WebApplicationFactory: open session → add item → finalize → close; fiado sem cliente 400."

- [ ] 11.3 `test:e2e` Parallel run logger (~4h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "Integração pilot tenant: finalize log diff ≤0.1% vs legacy seed."
  - **Spec:** `sale-finalization`

## 12. Acceptance verification

- [ ] 12.1 Validar US-060: abrir caixa PIN gerente Vue + Android
- [ ] 12.2 Validar US-061: scan, grade, reserva estoque
- [ ] 12.3 Validar US-062: finalize fiado/troco/comissão; parallel run 2 semanas ≤0,1%
- [ ] 12.4 Validar US-063: sangria + fechamento quebra
- [ ] 12.5 Validar US-064: cancelamento devolve estoque RN-060
- [ ] 12.6 Publicar contrato SaleCompleted para EP-007 Finance
