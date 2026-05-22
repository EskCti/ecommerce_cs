# Anti-Corruption Layer — Sistema de Vendas PHP → C# (RetailOps)

**Baseado em**: `docs/discovery/ecommerce-legado-php/domain-model.md` + `ddd-tactical-model.md`  
**Data**: 2026-05-22  
**Banco legado**: MariaDB `sas` (schema em `exemplos/php/sas.sql`)  
**Código legado**: `exemplos/php/vendas/` (`sistema/`, `sas/`, `rel/`, `rel_sistema/`)

---

## Princípios da ACL

1. **Domínio C# nunca** referencia nomes de tabela/coluna PHP (`empresa`, `senha_crip`, `itens_venda`).
2. Adapters Infrastructure **isolados** em `*.Infrastructure.Legacy` — removíveis após cutover.
3. Mappers são **bidirecionais** durante dual-write (Sales parallel run).
4. Status legado `Sim`/`Não` → enum `ActiveStatus` / `PaymentStatus` no domínio.
5. IDs int legado preservados como `LegacyId` até ETL final (compatibilidade FK).

---

## BC Identity & Access

### Mapeamento de dados

| Entidade legado | Campo legado | Conceito DDD | Tipo C# novo |
| --------------- | ------------ | ------------ | ------------ |
| `usuarios` | `id` | UserId | `UserId` (VO) / `LegacyUserId` |
| `usuarios` | `empresa` | TenantId | `TenantId` (0 = SAS) |
| `usuarios` | `nome` | PersonName | `PersonName` VO |
| `usuarios` | `email` | Email | `Email` VO |
| `usuarios` | `cpf` | Cpf | `Cpf` VO |
| `usuarios` | `senha_crip` | PasswordHash (legado) | `LegacyMd5Password` → migrar para `PasswordHash` |
| `usuarios` | `senha` | ManagerPin legado | `ManagerPin` VO (separar na reimplementação) |
| `usuarios` | `nivel` | UserLevel | enum `UserLevel` |
| `usuarios` | `ativo` | ActiveStatus | enum `ActiveStatus` |
| `usuarios` | `comissao` | CommissionRate | `CommissionRate` VO |
| `usuarios_permissoes` | `usuario`, `permissao` | PermissionGrant | entity filha de User |
| `acessos` | `chave` | PermissionKey | `PermissionKey` VO |
| `acessos` | `nome`, `grupo` | Permission metadata | read-only catalog |
| `cargos` | `nome` | Role label | `RoleName` VO |

### Traduções de regras

| Legado | Domínio C# |
| ------ | ---------- |
| Login email **ou** CPF + MD5 | `AuthenticateUserHandler` + `ILegacyPasswordVerifier` |
| Admin/SAS bypass permissões | `AuthorizationPolicy.IsPrivileged(level)` |
| Sem `usuarios_permissoes` → bloqueio | `AuthorizationPolicy.RequiresGrants()` |

### Componentes

| Classe | Responsabilidade |
| ------ | ---------------- |
| `LegacyUserAdapter` | EF/query `usuarios` + joins permissões |
| `LegacyUserMapper` | `LegacyUserRow` → `User` aggregate |
| `LegacyPermissionMapper` | `acessos` → `PermissionDefinition` |
| `LegacyMd5PasswordVerifier` | Verifica MD5; dispara `PasswordRehashRequested` |
| `IUserLegacyPort` | `FindByEmailOrCpf`, `Save`, `ListByTenant` |
| `LegacyAuthDbContext` | DbSets: `LegacyUsuario`, `LegacyPermissao`, … |

---

## BC Platform

### Mapeamento de dados

| Entidade legado | Campo legado | Conceito DDD | Tipo C# novo |
| --------------- | ------------ | ------------ | ------------ |
| `empresas` | `id` | CompanyId / TenantId | `CompanyId` |
| `empresas` | `nome` | CompanyName | `CompanyName` VO |
| `empresas` | `telefone`, `email` | Contact | `Phone`, `Email` VOs |
| `empresas` | `cpf`, `cnpj` | TaxDocument | `TaxDocument` VO |
| `empresas` | `ativo` | ActiveStatus | enum |
| `empresas` | `data_pgto` | NextBillingDate | `DueDate` VO |
| `empresas` | `valor` | MonthlyFee | `Money` VO |
| `empresas` | `teste` | TrialFlag | `TrialFlag` |
| `config` (empresa=0) | `dias_teste`, `dias_bloqueio`, `msg_bloqueio` | PlatformConfig | aggregate |
| `contratos` | `texto`, `data` | Contract | child of Company |
| `receber` | tipo=`Empresa`, `pessoa`=empresa.id | TenantInvoice | `PlatformReceivable` |

### Componentes

| Classe | Responsabilidade |
| ------ | ---------------- |
| `LegacyCompanyAdapter` | CRUD `empresas` |
| `LegacyCompanyMapper` | → `Company` aggregate |
| `LegacyPlatformConfigMapper` | `config` global → `PlatformConfig` |
| `LegacyTenantInvoiceMapper` | `receber` tipo Empresa → `TenantInvoice` |
| `IPlatformLegacyPort` | Operações SAS durante strangler |

---

## BC Store Settings

### Mapeamento de dados

| Entidade legado | Campo legado | Conceito DDD | Tipo C# novo |
| --------------- | ------------ | ------------ | ------------ |
| `config` (empresa>0) | `nome_sistema`, `cnpj_sistema`, … | StoreConfig | aggregate |
| `config` | `tipo_desconto` | DiscountType | enum `%` / `Fixed` |
| `config` | `comissao` | DefaultCommissionRate | VO |
| `config` | `tipo_rel`, `foto_rel` | ReportSettings | VO |
| `config` | `token` | WhatsApp ApiToken | VO |
| `forma_pgtos` | `nome`, `acrescimo` | PaymentMethod | aggregate |
| `caixas` | `nome`, `status`, `usuario` | CashRegisterTerminal | aggregate |

### Componentes

- `LegacyStoreConfigMapper`
- `LegacyPaymentMethodMapper`
- `LegacyCashRegisterTerminalMapper`
- `IStoreSettingsLegacyPort`

---

## BC CRM

### Mapeamento de dados

| Entidade legado | Campo legado | Conceito DDD | Tipo C# novo |
| --------------- | ------------ | ------------ | ------------ |
| `clientes` | `nome`, `cpf`, contatos | Customer | aggregate |
| `fornecedores` | `pessoa` F/J | Supplier + PersonType | aggregate |
| `arquivos` | tipo=`Cliente` | Attachment | child |

### Componentes

- `LegacyCustomerMapper` — `LegacyCustomerRow` → `Customer`
- `LegacySupplierMapper`
- `ICrmLegacyPort`

---

## BC Catalog & Inventory

### Mapeamento de dados

| Entidade legado | Campo legado | Conceito DDD | Tipo C# novo |
| --------------- | ------------ | ------------ | ------------ |
| `produtos` | `codigo` | Barcode | VO (unique/tenant) |
| `produtos` | `nome`, `descricao` | ProductName, Description | VOs |
| `produtos` | `estoque` | StockQuantity | VO |
| `produtos` | `valor_venda`, `valor_compra`, `lucro` | SalePrice, CostPrice, ProfitMargin | VOs |
| `produtos` | `nivel_estoque` | StockAlertLevel | VO |
| `produtos` | `ativo` | ActiveStatus | enum |
| `categorias` | `nome`, `ativo` | Category | aggregate |
| `cat_grade` | `nome` (Cor, Tamanho) | GradeDimension | entity |
| `itens_grade` | `texto`, `estoque` | GradeOption | entity |
| `entradas` / `saidas` | `motivo`, `quantidade` | StockMovement | aggregate/event |
| `detalhes_grade` | `tipo` Compra/Venda/Troca* | GradeMovementDetail | entity |

### Traduções críticas

| Legado | Domínio C# |
| ------ | ---------- |
| `valor_venda = 0` | `Product.OpenPrice = true` |
| Lucro % na compra | `ProfitMarginCalculator` no domínio |
| `detalhes_grade.tipo` | enum `MovementType` |

### Componentes

| Classe | Responsabilidade |
| ------ | ---------------- |
| `LegacyProductAdapter` | Join produto + grades |
| `LegacyProductMapper` | → `Product` aggregate tree |
| `LegacyStockMovementMapper` | entradas/saidas → `StockMovement` |
| `LegacyGradeDetailMapper` | detalhes_grade polimórfico |
| `IProductCatalogLegacyPort` | OHS para Sales: `FindByBarcode` |
| `IStockLegacyPort` | `AdjustStock`, `ReserveStock` |

---

## BC Sales (PDV) — ACL crítica

### Mapeamento de dados

| Entidade legado | Campo legado | Conceito DDD | Tipo C# novo |
| --------------- | ------------ | ------------ | ------------ |
| `caixas` | cadastro terminal | CashRegisterTerminal | ref Settings BC |
| `caixa` | sessão aberta/fechada | CashSession | aggregate root |
| `caixa` | `valor_ab`, `valor_fec`, `valor_quebra`, … | CashSession amounts | VOs |
| `itens_venda` | `venda = 0` | Cart / SaleLine (pending) | entity |
| `itens_venda` | `venda > 0` | SaleLine (confirmed) | entity |
| `receber` | tipo=`Venda` | Sale | aggregate root |
| `receber` | `id_ref` → `caixa.id` | CashSessionId | FK lógica |
| `receber` | `saida` | PaymentMethodName | string → VO ref |
| `receber` | `desconto`, `troco`, `acrescimo` | Discount, Change, Surcharge | VOs |
| `receber` | `pessoa` | CustomerId | nullable |
| `receber` | `vendedor` | SellerId | nullable |
| `sangrias` | `valor`, `id_caixa` | CashWithdrawal | entity |

### Fluxo ACL — FinalizeSale

```
Legacy PHP                          ACL (C#)                         Domínio C#
──────────                          ────────                         ──────────
buscar-codigo.php          →   LegacySaleAdapter.Finalize()    →   Sale.Complete()
  INSERT receber                  LegacySaleMapper.ToDomain()        SaleCompleted event
  UPDATE itens_venda              LegacySaleMapper.ToLegacy()        (dual-write log)
  INSERT comissoes                LegacyCommissionMapper
  UPDATE produtos.estoque    →   IStockLegacyPort (Catalog ACL)
```

### Componentes

| Classe | Responsabilidade |
| ------ | ---------------- |
| `LegacyCashSessionMapper` | `caixa` ↔ `CashSession` |
| `LegacyCartItemMapper` | `itens_venda` (venda=0) ↔ cart lines |
| `LegacySaleMapper` | `receber`+itens → `Sale` aggregate |
| `LegacySaleAdapter` | Orquestra transação compatível PHP |
| `ISalesLegacyPort` | `OpenSession`, `AddItem`, `Finalize`, `CloseSession` |
| `SaleParallelRunLogger` | Compara totais PHP shadow vs C# |

### Invariantes a preservar na ACL

- Quebra: `valor_fec - (valor_ab + valor_vendido - valor_sangrias)`
- Fiado: `pessoa` obrigatório se vencimento > hoje
- Troco ≥ 0
- Comissão: `total_sem_taxa * percent / 100`

---

## BC Finance

### Mapeamento de dados

| Entidade legado | Campo legado | Conceito DDD | Tipo C# novo |
| --------------- | ------------ | ------------ | ------------ |
| `receber` | tipo=`Venda` | SaleReceivable | **não** misturar com manual |
| `receber` | tipo=`Empresa` | TenantInvoice | Platform BC |
| `receber` | outros | ManualReceivable | aggregate |
| `receber` | `pago`, datas | PaymentStatus, settlement | VOs |
| `pagar` | tipo=`Compra` | PurchasePayable | aggregate |
| `pagar` | tipo=`Pagamento` | CommissionPayment | aggregate |
| `pagar` | tipo=`Conta` | ExpensePayable | aggregate |
| `comissoes` | `id_ref` venda, `pago` | Commission | aggregate |
| `frequencias` | `dias` | Recurrence | VO |
| `arquivos` | anexos | Attachment | entity |

### Discriminator legado → C# (STI)

```csharp
// Infrastructure.Legacy — não vazar para Domain
public enum LegacyReceivableType { Venda, Empresa, Other }
// Mapper roteia:
//   Venda   → ISaleReceivableFactory (Sales BC)
//   Empresa → IPlatformInvoiceFactory (Platform BC)
//   Other   → Receivable.CreateManual(...)
```

### Componentes

- `LegacyReceivableMapper` (discriminator `tipo`)
- `LegacyPayableMapper`
- `LegacyCommissionMapper`
- `IFinanceLegacyPort`

---

## BC Returns

### Mapeamento de dados

| Entidade legado | Campo legado | Conceito DDD | Tipo C# novo |
| --------------- | ------------ | ------------ | ------------ |
| `trocas` | produto_entrada/saida | Exchange | aggregate |
| `trocas` | cliente | CustomerId | VO |
| `detalhes_grade` | Troca Entrada/Saída | GradeMovementDetail | 2 registros |

### Componentes

- `LegacyExchangeMapper`
- `LegacyExchangeAdapter` — chama `IStockLegacyPort` ±1
- `IReturnsLegacyPort`

---

## BC Reporting

### Mapeamento de queries legado

| Script legado | Query C# nova | Read model |
| ------------- | ------------- | ---------- |
| `rel_sistema/vendas_class.php` | `ReportSalesQuery` | `SalesReportRow` |
| `rel_sistema/receber_class.php` | `ReportReceivablesQuery` | `ReceivablesReportRow` |
| `rel_sistema/lucro_class.php` | `ReportProfitQuery` | `ProfitStatement` |
| `rel_sistema/estoque_class.php` | `ReportLowStockQuery` | `LowStockRow` |
| `rel_sistema/comprovante.php` | `GenerateReceiptQuery` | `ReceiptDocument` |
| `rel/empresas_class.php` | `ReportTenantsQuery` | Platform scope |

### Componentes

- `LegacyReportSqlAdapter` — executa SQL legado temporário (somente leitura)
- `LegacyReportMapper` — rows → DTOs CQRS
- `PdfRendererAdapter` — substitui dompdf (QuestPDF recomendado)

---

## BC Notifications

### Mapeamento

| Legado | Domínio C# |
| ------ | ---------- |
| `api/mensagem.php` job | `SendDailyTenantDigestHandler` |
| `api/agendar.php` | `ScheduleWhatsAppMessageHandler` |
| `config.token`, `telefone_sistema` | `WhatsAppCredentials` VO |

### Componentes

- `EnviameWhatsAppAdapter` — ACL HTTP (substitui `file_get_contents`)
- `LegacyDigestQueryAdapter` — agrega via `IFinanceLegacyPort` + `ICatalogLegacyPort`

---

## Diagrama ACL global

```
┌────────────────────────────────────────────────────────────────────┐
│                     ASP.NET Core — Domain Layer                       │
│   User │ Company │ Product │ Sale │ Receivable │ Customer │ …      │
└───────────────────────────────┬────────────────────────────────────┘
                                │ Ports (interfaces Application)
┌───────────────────────────────▼────────────────────────────────────┐
│              Infrastructure.Legacy (temporary)                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌────────────┐ │
│  │ Auth Mapper │  │Catalog Mapper│  │ Sales Mapper│  │Finance Map │ │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘  └─────┬──────┘ │
│         └────────────────┴────────────────┴───────────────┘         │
│                              │                                       │
│                    LegacySasDbContext (EF Core)                       │
└───────────────────────────────┬────────────────────────────────────┘
                                │
                    ┌───────────▼───────────┐
                    │   MariaDB `sas`       │
                    │   (schema legado)     │
                    └───────────────────────┘
```

---

## Cronograma de remoção da ACL

| BC | Remover ACL quando |
| -- | ------------------ |
| Identity | 100% users com bcrypt; sem login PHP |
| Platform | `empresas` migrada para schema `platform_*` |
| Catalog | Estoque validado; tabelas `produtos_*` normalizadas |
| Sales | Parallel run OK; `sales_*` schema owns transactions |
| Finance | Receber/pagar split por tipo em tabelas dedicadas |
| Demais | Após cutover do BC |

**Pós-ACL**: `Infrastructure.Legacy` deleted; apenas `Infrastructure` com migrations EF Core normais (`config-efcore-cs`).

---

## Skills C# por componente ACL

| Componente | Skill |
| ---------- | ----- |
| Legacy DbContext + Fluent mappings | `backend-data-cs` |
| Mappers / Adapters | `core-repository-cs` |
| Ports no Application | `core-use-case-cs` |
| DTOs de linha legado | `core-dto-cs` |
| Testes mapper | `test-unit-cs` |
| E2E strangler routing | `test-e2e-cs` |
