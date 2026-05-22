# Análise DDD / Clean Architecture — Sistema de Vendas (Legado PHP)

**Baseado em**: `requirements.md` + análise de `exemplos/php/vendas/` e `sas.sql`  
**Data da análise**: 2026-05-22

## Context Map

```
┌──────────────────────────────────────────────────────────────────┐
│                        CONTEXT MAP                               │
│                                                                  │
│  ┌─────────────┐         cobrança / bloqueio                     │
│  │  Platform   │──────────────────────────────────▶ Tenant Ops    │
│  │   (SaaS)    │         upstream                          │     │
│  └──────┬──────┘                                           │     │
│         │ shared kernel: User, CompanyId, Money              │     │
│         ▼                                                  ▼     │
│  ┌─────────────┐    catálogo/preço    ┌─────────────┐           │
│  │  Identity   │◀────────────────────▶│   Catalog   │           │
│  │  & Access   │                      │  & Inventory│           │
│  └──────┬──────┘                      └──────┬──────┘           │
│         │                                    │                   │
│         │         ┌─────────────┐            │                   │
│         └────────▶│    Sales    │◀───────────┘                   │
│                   │   (PDV)     │                                │
│                   └──────┬──────┘                                │
│                          │ gera receivable / commission           │
│                          ▼                                       │
│                   ┌─────────────┐      ┌─────────────┐           │
│                   │  Finance    │◀────▶│  Purchasing │           │
│                   └──────┬──────┘      └─────────────┘           │
│                          │                                       │
│         ┌────────────────┼────────────────┐                      │
│         ▼                ▼                ▼                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │  Returns    │  │  Reporting  │  │ Notifications│             │
│  │ (Exchanges) │  │   (read)    │  │  (WhatsApp)  │             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
│                                                                  │
│  Relações: ──▶ upstream/downstream                               │
│            ◀──▶ shared kernel (TenantId, UserId, Money)          │
│            ···▶ conformist (Reporting lê modelos existentes)     │
└──────────────────────────────────────────────────────────────────┘
```

## Bounded Contexts

### BC-001: Platform (SaaS)

**Descrição**: Operação multi-tenant — onboarding, mensalidade, contratos, config global.  
**Relação**: upstream de Tenant Operations; conformist com Finance (cobrança).

#### Entities

| Entity | Atributos Principais | Aggregate Root? |
| ------ | -------------------- | --------------- |
| Company (empresas) | id, nome, contatos, CNPJ/CPF, ativo, teste, data_pgto, valor | Sim |
| PlatformConfig (config empresa=0) | dias_teste, dias_bloqueio, msg_bloqueio, token | Sim |
| Contract (contratos) | empresa, texto HTML, data | Não (child de Company) |
| PlatformReceivable | receber tipo Empresa, pessoa→companyId | Não |

#### Value Objects

| VO | Tipo | Validação |
| -- | ---- | --------- |
| MonthlyFee | decimal | >= 0 |
| TrialPeriod | int dias | > 0 |
| CompanyDocument | CPF/CNPJ | formato BR |
| BlockPolicy | dias + mensagem | dias >= 0 |

#### Domain Services

| Service | Regra |
| ------- | ----- |
| TrialExpirationPolicy | Desativa company + users quando teste expirado |
| TenantBillingPolicy | Gera cobrança recorrente; dispara bloqueio após dias_bloqueio |

#### Use Cases

| Use Case | Tipo | Descrição |
| -------- | ---- | --------- |
| RegisterTrialCompany | comando | Self-service trial |
| CreateCompany | comando | SAS cria tenant + admin |
| SuspendOverdueTenant | comando | Bloqueio por inadimplência |
| GenerateContract | comando | Template contrato locação software |

---

### BC-002: Identity & Access

**Descrição**: Autenticação, usuários, RBAC, cargos.  
**Relação**: shared kernel com todos os BCs tenant-scoped.

#### Entities

| Entity | Atributos Principais | Aggregate Root? |
| ------ | -------------------- | --------------- |
| User (usuarios) | empresa, nome, email, cpf, nivel, ativo, comissao | Sim |
| PermissionGrant (usuarios_permissoes) | usuario, permissao | Não |
| Access (acessos) | chave, nome, grupo | Entidade catálogo |
| AccessGroup (grupo_acessos) | id, nome | Entidade catálogo |
| Role (cargos) | nome, empresa | Entidade referência |

#### Value Objects

| VO | Tipo | Validação |
| -- | ---- | --------- |
| Email | string | formato email |
| Cpf | string | formato CPF |
| PasswordHash | string | bcrypt/argon na reimplementação |
| UserLevel | enum | SAS, Administrador, Gerente, Operador, Vendedor |
| PermissionKey | string | ex.: produtos, abertura, rel_vendas |

#### Domain Services

| Service | Regra |
| ------- | ----- |
| AuthorizationPolicy | Admin/SAS bypass; demais exigem PermissionGrant |
| ManagerAuthentication | Valida credencial gerente para caixa |

#### Use Cases

| Use Case | Tipo | Descrição |
| -------- | ---- | --------- |
| AuthenticateUser | comando | Login email/CPF + senha |
| AssignPermissions | comando | CRUD usuarios_permissoes |
| DeactivateUser | comando | mudar-status |

---

### BC-003: Catalog & Inventory

**Descrição**: Produtos, categorias, variações (grade), movimentação de estoque.  
**Relação**: upstream de Sales; shared kernel ProductId/SKU com Purchasing.

#### Entities

| Entity | Atributos Principais | Aggregate Root? |
| ------ | -------------------- | --------------- |
| Product (produtos) | codigo, nome, estoque, valor_venda/compra, categoria, fornecedor, nivel_estoque | Sim |
| Category (categorias) | nome, ativo | Sim |
| GradeDimension (cat_grade) | produto, nome (Cor, Tamanho) | Não |
| GradeItem (itens_grade) | cat_grade, texto, estoque | Não |
| StockMovement (entradas/saidas) | produto, quantidade, motivo, usuario | Sim (event log) |
| GradeMovementDetail (detalhes_grade) | produto, tipo, quantidade, grades | Não |

#### Value Objects

| VO | Tipo | Validação |
| -- | ---- | --------- |
| Barcode | string | único por tenant |
| StockQuantity | int | >= 0 |
| SalePrice | Money | >= 0 ou 0=preço PDV |
| StockAlertLevel | int | >= 0 |
| MovementReason | string | não vazio |

#### Domain Services

| Service | Regra |
| ------- | ----- |
| StockAdjustmentPolicy | Entrada/saída manual com motivo |
| LowStockPolicy | estoque < nivel_estoque |
| ProfitMarginCalculator | lucro % na compra |

#### Aggregates

```
Product (root)
├── GradeDimension[]
│   └── GradeItem[] (estoque por variação)
└── StockMovement[] (via domain events)

GradeMovementDetail → referencia Product + Sale/Purchase/Exchange
```

#### Use Cases

| Use Case | Tipo | Descrição |
| -------- | ---- | --------- |
| CreateProduct | comando | CRUD produto |
| RegisterStockEntry | comando | entrada manual |
| RegisterStockExit | comando | saída manual |
| PurchaseStock | comando | compra + pagar + grade detail |
| ConfigureProductGrade | comando | cat_grade + itens_grade |

---

### BC-004: Sales (PDV)

**Descrição**: Caixa, carrinho, finalização, sangria, comprovante.  
**Relação**: downstream de Catalog; upstream de Finance (receivable, commission).

#### Entities

| Entity | Atributos Principais | Aggregate Root? |
| ------ | -------------------- | --------------- |
| CashRegister (caixas) | nome, status, usuario | Sim (cadastro) |
| CashSession (caixa) | abertura/fechamento, valores, operador, gerente | Sim |
| SaleCartItem (itens_venda) | produto, qty, valor_unitario, venda(0=carrinho) | Não |
| Sale (receber tipo Venda) | valor, pagamento, cliente, vendedor, desconto | Sim |
| CashWithdrawal (sangrias) | valor, id_caixa | Não |

#### Value Objects

| VO | Tipo | Validação |
| -- | ---- | --------- |
| Discount | % ou Money | conforme config tenant |
| PaymentMethod | string | forma_pgtos + acrescimo |
| ChangeAmount | Money | troco >= 0 |
| CashBreakage | Money | calculado no fechamento |
| WarrantyDays | int | dias garantia |

#### Domain Services

| Service | Regra |
| ------- | ----- |
| SaleFinalizationPolicy | Fiado exige cliente; troco >= 0 |
| CartStockReservation | Abate estoque ao adicionar item |
| CommissionCalculator | % vendedor ou config loja × total_sem_taxa |
| CashSessionClosingPolicy | quebra = fechamento - (abertura + vendas - sangrias) |

#### Use Cases

| Use Case | Tipo | Descrição |
| -------- | ---- | --------- |
| OpenCashSession | comando | abertura + senha gerente |
| AddItemToCart | comando | scan código |
| ConfirmGradeForItem | comando | detalhes_grade Venda |
| FinalizeSale | comando | receber + link items + commission |
| RemoveCartItem | comando | devolve estoque |
| RegisterCashWithdrawal | comando | sangria |
| CloseCashSession | comando | fechamento + quebra |
| CancelSale | comando | excluir venda + estorno estoque |

---

### BC-005: Returns (Exchanges)

**Descrição**: Troca produto entrada/saída.  
**Relação**: downstream de Catalog; usa CRM Customer.

#### Entities

| Entity | Atributos Principais | Aggregate Root? |
| ------ | -------------------- | --------------- |
| Exchange (trocas) | cliente, produto_entrada, produto_saida, usuario | Sim |

#### Use Cases

| Use Case | Tipo | Descrição |
| -------- | ---- | --------- |
| RegisterExchange | comando | movimenta estoque ±1 + grade details |

---

### BC-006: Finance

**Descrição**: Contas receber/pagar, fluxo, comissões, recorrência.  
**Relação**: downstream de Sales e Platform; shared Money/Frequency.

#### Entities

| Entity | Atributos Principais | Aggregate Root? |
| ------ | -------------------- | --------------- |
| Receivable (receber) | tipo, valor, vencimento, pago, pessoa | Sim |
| Payable (pagar) | tipo, valor, vencimento, pago, pessoa | Sim |
| Commission (comissoes) | vendedor, valor, id_ref venda, pago | Sim |
| Frequency (frequencias) | dias, label | Entidade referência |
| Attachment (arquivos) | tipo, id_ref, foto | Não |

#### Value Objects

| VO | Tipo | Validação |
| -- | ---- | --------- |
| Money | decimal(8,2) | >= 0 |
| DueDate | date | |
| PaymentStatus | enum | Sim/Não |
| Recurrence | int dias | referencia frequencias |
| AccountType | enum | Venda, Compra, Empresa, Conta, Pagamento |

#### Domain Services

| Service | Regra |
| ------- | ----- |
| PayableSettlement | Baixa registra data_pgto + usuario_pgto |
| CommissionSettlement | Baixa gera pagar tipo Pagamento |
| CashFlowAggregator | Soma receber/pagar por período |

#### Use Cases

| Use Case | Tipo | Descrição |
| -------- | ---- | --------- |
| CreateReceivable | comando | manual ou via FinalizeSale |
| SettleReceivable | comando | baixar |
| CreatePayable | comando | despesa/compra |
| SettlePayable | comando | baixar |
| PayCommission | comando | baixar comissão(ões) |
| GetCashFlow | query | fluxo período |

---

### BC-007: CRM (Parties)

**Descrição**: Clientes, fornecedores — partes envolvidas em vendas/compras.  
**Relação**: downstream consumer de Sales/Purchasing.

#### Entities

| Entity | Atributos Principais | Aggregate Root? |
| ------ | -------------------- | --------------- |
| Customer (clientes) | nome, cpf, contatos | Sim |
| Supplier (fornecedores) | nome, cpf/cnpj, pessoa F/J | Sim |

#### Use Cases

| Use Case | Tipo | Descrição |
| -------- | ---- | --------- |
| CreateCustomer | comando | CRUD |
| FindOrCreateCustomerByCpf | comando | usado em trocas/fiado |
| CreateSupplier | comando | CRUD |

---

### BC-008: Reporting

**Descrição**: Projeções read-only para PDF.  
**Relação**: conformist — lê dados de Sales, Finance, Catalog.

#### Use Cases (Queries)

| Use Case | Descrição |
| -------- | --------- |
| ReportSales | vendas por período/vendedor |
| ReportReceivables | recebimentos |
| ReportPayables | despesas |
| ReportProfit | demonstrativo lucro |
| ReportLowStock | produtos abaixo nível |
| ReportCashSessions | caixas abertos/fechados |
| ReportCommissions | comissões |
| ReportExchanges | trocas |
| ReportCustomers | clientes |
| GenerateReceipt | comprovante venda |

---

### BC-009: Notifications

**Descrição**: Mensageria WhatsApp.  
**Relação**: downstream de Platform + Finance + Catalog (alertas).

#### Use Cases

| Use Case | Tipo | Descrição |
| -------- | ---- | --------- |
| SendDailyTenantDigest | comando | contas hoje + estoque baixo + cobrança |
| ScheduleWhatsAppMessage | comando | agendar via API |

---

## Mapeamento de Camadas (Clean Architecture)

| Camada | Artefatos Identificados (legado → alvo) | Skill Agnóstico |
| ------ | --------------------------------------- | --------------- |
| Domain | Product, Sale, CashSession, Receivable, User, Company, Exchange, Commission | core-entity, core-value-object, core-domain-service |
| Application | Scripts `salvar.php`, `baixar.php`, `buscar-codigo.php` → Use Cases | core-use-case, core-dto, core-query-cqrs |
| Infrastructure | PDO queries, uploads, dompdf, WhatsApp HTTP | backend-prisma-data / backend-data-kt / backend-data-cs |
| Interface | `index.php?pagina=*`, AJAX POST endpoints | backend-controller, frontend-form-schema / frontend-page-* |

## Dependências entre Contexts

| De | Para | Tipo | Dados Compartilhados |
| -- | ---- | ---- | -------------------- |
| Platform | Identity | upstream | CompanyId, User provisioning |
| Sales | Catalog | downstream | ProductId, stock decrement |
| Sales | Finance | downstream | Sale → Receivable, Commission |
| Sales | CRM | downstream | CustomerId (fiado) |
| Purchasing | Catalog | downstream | stock increment, cost price |
| Purchasing | Finance | downstream | Payable tipo Compra |
| Returns | Catalog + CRM | downstream | stock ±1, CustomerId |
| Reporting | * | conformist | read models |
| Notifications | Platform + Finance + Catalog | downstream | billing, due dates, low stock |

## Recomendações Arquiteturais

1. **TenantId como cross-cutting concern** — middleware/guard em toda request; equivalente ao filtro `empresa` atual.
2. **Unificar Sale aggregate** — hoje `receber` + `itens_venda` + `detalhes_grade`; na reimplementação, `Sale` root com `SaleLine` e `Payment`.
3. **CashSession como aggregate** — encapsular abertura, itens pendentes (carrinho), sangrias e fechamento.
4. **Separar Platform BC** — módulo/deploy distinto do tenant app (como já existe `sas/` vs `sistema/`).
5. **Substituir MD5/plaintext** — PasswordHash VO + JWT/session segura; PIN gerente separado de senha login.
6. **Eventos de domínio** — `SaleCompleted`, `StockAdjusted`, `TenantSuspended` para desacoplar Finance e Notifications.
7. **Reporting como CQRS** — queries dedicadas; não replicar SQL inline dos `*_class.php`.
8. **Não replicar anti-patterns** — SQL concatenado, ausência de transações explícitas nos fluxos PDV.
9. **E-commerce futuro** — se vitrine B2C for requisito novo, tratar como BC separado (Online Store) integrado via Catalog shared kernel — **não existe no legado**.

## Mapeamento Módulo Legado → BC

| Pasta / Tabela legado | Bounded Context |
| --------------------- | --------------- |
| `vendas/sas/` | Platform |
| `vendas/index.php`, `autenticar.php` | Identity & Access |
| `sistema/paginas/usuarios`, `cargos`, `acessos` | Identity & Access |
| `sistema/paginas/produtos`, `categorias`, `entradas`, `saidas`, `estoque` | Catalog & Inventory |
| `sistema/paginas/pdv`, `abertura`, `caixas` | Sales (PDV) |
| `sistema/paginas/trocas` | Returns |
| `sistema/paginas/receber`, `pagar`, `compras`, `vendas`, `fluxo`, `comissoes` | Finance |
| `sistema/paginas/clientes`, `fornecedores`, `funcionarios` | CRM |
| `rel/`, `rel_sistema/` | Reporting |
| `api/mensagem.php` | Notifications |

## Endpoints HTTP (mapeamento para reimplementação)

| Verbo | Rota legado (padrão) | Use Case |
| ----- | -------------------- | -------- |
| POST | `autenticar.php` | AuthenticateUser |
| POST | `cadastrar.php` | RegisterTrialCompany |
| POST | `sistema/paginas/pdv/buscar-codigo.php` | AddItemToCart / FinalizeSale |
| POST | `sistema/paginas/abertura/salvar.php` | OpenCashSession |
| POST | `sistema/paginas/pdv/fechar-caixa.php` | CloseCashSession |
| POST | `sistema/paginas/produtos/salvar.php` | CreateProduct / UpdateProduct |
| POST | `sistema/paginas/produtos/comprar.php` | PurchaseStock |
| POST | `sistema/paginas/trocas/salvar.php` | RegisterExchange |
| POST | `sistema/paginas/vendas/excluir.php` | CancelSale |
| POST | `sistema/paginas/receber/baixar.php` | SettleReceivable |
| POST | `sas/paginas/empresas/salvar.php` | CreateCompany |
| POST | `rel_sistema/vendas_class.php` | ReportSales (PDF) |
