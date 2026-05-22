## Why

Antes do catálogo (EP-005) e PDV (EP-006), cada tenant precisa configurar a loja, formas de pagamento e terminais de caixa — hoje em `editar-config.php`, `forma_pgtos/` e `caixas/`. O BC Store Settings é **pré-requisito do MVP** (Release 1) e supporting domain que desbloqueia vendas com acréscimo de parcelamento (RN-047) e sessões de caixa (EP-006).

## What Changes

- Novo módulo `RetailOps.StoreSettings` (Core, Application, Infrastructure)
- Agregados `StoreConfig` (1 por tenant), `PaymentMethod`, `CashRegisterTerminal`
- VOs: `StoreName`, `Cnpj`, `DiscountType`, `CommissionRate`, `ReportFormat`, `ApiToken`, `ImagePath`, surcharge percent
- Serviço de domínio `PaymentSurchargeCalculator` (RN-047)
- ACL legado: `config` (empresa>0), `forma_pgtos`, `caixas` via `IStoreSettingsLegacyPort`
- API tenant-scoped: CRUD configuração loja, formas de pagamento, caixas físicos
- Frontend Vue: formulário configuração + listagens/forms PaymentMethod e CashRegisterTerminal
- Testes unitários ≥95% e E2E CRUD settings
- **Sem mobile** neste épico (admin web only)

## Capabilities

### New Capabilities

- `store-config`: Configuração da loja por tenant (nome, logo, CNPJ, endereço, desconto, comissão, relatório, token)
- `payment-methods`: CRUD formas de pagamento com percentual de acréscimo
- `cash-register-terminals`: CRUD terminais de caixa físico (nome, status, operador vinculado)
- `store-settings-web-ui`: UI Vue admin tenant para config, formas pgto e caixas

### Modified Capabilities

_(nenhuma — BC novo; consumo futuro por Sales EP-006 via IDs/referências, não altera specs existentes)_

## Impact

- **Backend**: módulo StoreSettings + migrations; ACL dual-write em parallel run
- **Frontend**: rotas tenant `/settings/*` com guards de permissão EP-001
- **Sales BC (EP-006)**: depende de `PaymentMethod` e `CashRegisterTerminal` existentes
- **Dependências**: EP-000 bootstrap, EP-001 auth (admin tenant)
- **Platform BC**: distinto de `config` empresa=0 (PlatformConfig permanece EP-002)
