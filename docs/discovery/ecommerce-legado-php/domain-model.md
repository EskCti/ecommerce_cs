# Modelo de Domínio — Sistema de Vendas (Legado PHP)

**Fonte**: `exemplos/php/sas.sql` + scripts em `exemplos/php/vendas/`  
**Data da análise**: 2026-05-22

## Visão do Schema

Banco único `sas` — 24 tabelas principais. Multi-tenancy via coluna `empresa` (0 = plataforma SAS).

## Entidades por Bounded Context

### BC Platform

#### Company (`empresas`)

| Campo | Tipo | Obrigatório | VO candidato |
| ----- | ---- | ----------- | -------------- |
| id | int PK | sim | CompanyId |
| nome | varchar(50) | sim | CompanyName |
| telefone | varchar(20) | sim | Phone |
| email | varchar(50) | não | Email |
| cpf / cnpj | varchar(20) | não | TaxDocument |
| ativo | Sim/Não | sim | ActiveStatus |
| data_cad | date | não | |
| data_pgto | date | não | NextBillingDate |
| valor | decimal(8,2) | não | MonthlyFee |
| endereco | varchar(100) | não | Address |
| teste | Sim/Não | não | TrialFlag |

- **Aggregate Root**: sim  
- **Relações**: 1:N User, Contract, Config (empresa>0)  
- **Regras**: trial expirado desativa company + users

#### PlatformConfig (`config` where empresa=0)

| Campo | Tipo | VO candidato |
| ----- | ---- | -------------- |
| dias_teste | int | TrialDays |
| dias_bloqueio | int | BlockAfterDays |
| msg_bloqueio | varchar(255) | BlockMessage |
| teste | Sim/Não | TrialEnabled |
| horario_mensagens | time | ScheduleTime |
| token | varchar(100) | ApiToken |

#### Contract (`contratos`)

| Campo | Tipo | Observação |
| ----- | ---- | ---------- |
| empresa | FK | |
| texto | text HTML | template locação software |
| data | date | |

---

### BC Identity & Access

#### User (`usuarios`)

| Campo | Tipo | Obrigatório | VO candidato |
| ----- | ---- | ----------- | -------------- |
| id | int PK | sim | UserId |
| empresa | int FK | sim | TenantId |
| nome | varchar(50) | sim | PersonName |
| email / cpf | varchar | login | Email, Cpf |
| senha / senha_crip | varchar | sim | Password (legado MD5) |
| nivel | varchar(50) | sim | UserLevel |
| ativo | Sim/Não | sim | ActiveStatus |
| comissao | int % | não | CommissionRate |
| foto | varchar(100) | sim | ImagePath |

- **Aggregate Root**: sim  
- **Relações**: N:M Access via `usuarios_permissoes`

#### PermissionGrant (`usuarios_permissoes`)

| Campo | Tipo |
| ----- | ---- |
| id | PK |
| usuario | FK usuarios |
| permissao | FK acessos |

#### Access (`acessos`) — catálogo global

| Campo | Tipo | Exemplos chave |
| ----- | ---- | -------------- |
| chave | varchar(50) | home, produtos, abertura, rel_vendas |
| grupo | int | FK grupo_acessos |

---

### BC Catalog & Inventory

#### Product (`produtos`)

| Campo | Tipo | Obrigatório | VO candidato |
| ----- | ---- | ----------- | -------------- |
| id | int PK | sim | ProductId |
| empresa | int | sim | TenantId |
| codigo | varchar(50) | sim | Barcode |
| nome | varchar(100) | sim | ProductName |
| descricao | varchar(255) | não | |
| estoque | int | não | StockQuantity |
| valor_venda | decimal | não | Money (0=PDV informa) |
| valor_compra | decimal | não | Money |
| lucro | decimal | não | Percentage |
| fornecedor | int FK | sim | SupplierId |
| categoria | int FK | sim | CategoryId |
| nivel_estoque | int | não | StockAlertLevel |
| ativo | Sim/Não | sim | ActiveStatus |
| foto | varchar(100) | sim | ImagePath |

- **Aggregate Root**: sim  
- **Relações**: N:1 Category, Supplier; 1:N cat_grade; movimentações via entradas/saidas/detalhes_grade

#### Category (`categorias`)

| Campo | Tipo |
| ----- | ---- |
| nome | varchar(50) |
| ativo | Sim/Não |
| empresa | TenantId |

#### GradeDimension (`cat_grade`)

| Campo | Tipo |
| ----- | ---- |
| produto | FK |
| nome | Cor, Tamanho, Numeração |

#### GradeItem (`itens_grade`)

| Campo | Tipo |
| ----- | ---- |
| cat_grade | FK |
| texto | ex.: "P", "Azul" |
| estoque | int |

#### StockMovement — `entradas` / `saidas`

| Campo | Tipo |
| ----- | ---- |
| produto | FK |
| quantidade | int |
| motivo | varchar(255) |
| usuario | FK |
| data | date |
| empresa | TenantId |

#### GradeMovementDetail (`detalhes_grade`)

| Campo | Tipo | Valores tipo |
| ----- | ---- | ------------ |
| produto | FK | |
| id_ref | int | id venda/compra/troca/item |
| quantidade | int | |
| cat_grade, cat_grade2 | FK | dimensões |
| itens_grade, itens_grade2 | FK | variações |
| tipo | varchar(20) | Compra, Venda, Troca Entrada, Troca Saída |

---

### BC Sales (PDV)

#### CashRegister (`caixas`) — cadastro físico

| Campo | Tipo |
| ----- | ---- |
| nome | varchar(50) |
| status | Aberto/Fechado |
| usuario | operador atual |
| empresa | TenantId |

#### CashSession (`caixa`) — sessão operacional

| Campo | Tipo | Descrição |
| ----- | ---- | --------- |
| data_ab / hora_ab | date/time | abertura |
| valor_ab | decimal | fundo de troco |
| gerente_ab | FK | quem autorizou |
| data_fec / hora_fec | date/time | fechamento |
| valor_fec | decimal | contado |
| valor_vendido | decimal | soma vendas sessão |
| valor_sangrias | decimal | |
| valor_quebra | decimal | diferença |
| operador | FK | |
| caixa | FK caixas | |
| status | Aberto/Fechado | |

- **Aggregate Root**: sim (sessão)

#### SaleCartItem (`itens_venda`)

| Campo | Tipo | Observação |
| ----- | ---- | ---------- |
| produto | FK | |
| valor_unitario | decimal | |
| quantidade | int | |
| usuario | FK | operador |
| venda | int | 0 = carrinho aberto; >0 = receber.id |
| data | date | |

#### Sale (`receber` where tipo='Venda')

| Campo | Tipo | VO candidato |
| ----- | ---- | -------------- |
| valor | decimal | Money |
| valor_recebido | decimal | Money |
| desconto | decimal | Discount |
| troco | decimal | ChangeAmount |
| acrescimo | int | PaymentSurchargePercent |
| saida | varchar | PaymentMethod name |
| pessoa | FK clientes | CustomerId |
| vendedor | FK usuarios | SellerId |
| garantia | int | WarrantyDays |
| id_ref | FK caixa | CashSessionId |
| pago | Sim/Não | PaymentStatus |
| data_venc / data_pgto | date | |
| hora | time | |

- **Aggregate Root**: sim (com SaleLine implícito em itens_venda)

#### CashWithdrawal (`sangrias`)

| Campo | Tipo |
| ----- | ---- |
| valor | decimal |
| id_caixa | FK caixa sessão |
| empresa | TenantId |

---

### BC Returns

#### Exchange (`trocas`)

| Campo | Tipo |
| ----- | ---- |
| cliente | FK |
| produto_entrada | FK produtos |
| produto_saida | FK produtos |
| usuario | FK |
| data | date |
| empresa | TenantId |

- **Aggregate Root**: sim  
- **Regras**: sempre qty=1; gera 2 detalhes_grade

---

### BC Finance

#### Receivable (`receber`) — tipos: Venda, Empresa, manual

Ver Sale acima + campos genéricos:

| tipo | Uso |
| ---- | --- |
| Venda | PDV |
| Empresa | cobrança SaaS (empresa=0, pessoa=tenantId) |
| (outros) | receitas manuais tenant |

#### Payable (`pagar`) — tipos: Compra, Conta, Pagamento, Empresa

| Campo | Tipo |
| ----- | ---- |
| tipo | varchar(30) |
| descricao | varchar(50) |
| pessoa | FK fornecedor/funcionário/0 |
| valor | decimal |
| frequencia | FK |
| pago | Sim/Não |
| id_ref | FK produto/compra/etc |

#### Commission (`comissoes`)

| Campo | Tipo |
| ----- | ---- |
| descricao | varchar(50) |
| valor | decimal |
| vendedor | FK usuarios |
| id_ref | FK receber (venda) |
| pago | Sim/Não |
| data_lanc / data_pgto | date |

#### Frequency (`frequencias`)

| Campo | Tipo | Exemplos |
| ----- | ---- | -------- |
| frequencia | varchar(30) | Nenhuma, Diária, Semanal, Mensal |
| dias | int | 0, 1, 7, 30 |

#### PaymentMethod (`forma_pgtos`)

| Campo | Tipo |
| ----- | ---- |
| nome | Dinheiro, Pix, Fiado, Cartão... |
| acrescimo | int % parcelamento |

#### Attachment (`arquivos`)

| Campo | Tipo |
| ----- | ---- |
| tipo | Empresa, Cliente, Receber, Pagar |
| id_ref | int |
| foto | path arquivo |
| data_validade | date opcional |

---

### BC CRM

#### Customer (`clientes`)

| Campo | Tipo | VO |
| ----- | ---- | -- |
| nome | varchar(50) | PersonName |
| cpf | varchar(20) | Cpf |
| telefone, email, endereco | | contato |

#### Supplier (`fornecedores`)

| Campo | Tipo |
| ----- | ---- |
| pessoa | Física/Jurídica |
| cpf | documento |

---

## Diagrama de Aggregates

```
┌─────────────────────────────────────────────────────────────┐
│ Company (Platform)                                            │
│   ├── Contract[]                                              │
│   └── PlatformReceivable[] (receber empresa=0)               │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ User (Identity)                                               │
│   └── PermissionGrant[]                                       │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ Product (Catalog)                                             │
│   ├── GradeDimension[]                                        │
│   │     └── GradeItem[]                                       │
│   └── (StockMovement via domain events)                       │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ CashSession (Sales)                                           │
│   ├── SaleCartItem[] (venda=0)                                │
│   ├── Sale[] (receber)                                        │
│   │     └── SaleLine[] (itens_venda)                          │
│   │           └── GradeMovementDetail?                        │
│   └── CashWithdrawal[] (sangrias)                             │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ Exchange (Returns)                                            │
│   ├── GradeMovementDetail (entrada)                           │
│   └── GradeMovementDetail (saída)                             │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ Receivable / Payable / Commission (Finance)                   │
│   └── Attachment[]                                            │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ Customer / Supplier (CRM)                                     │
│   └── Attachment[] (Cliente)                                  │
└─────────────────────────────────────────────────────────────┘
```

## Mapeamento Entity ↔ Tabela

| Entity (DDD) | Tabela legado | Observações |
| ------------ | ------------- | ----------- |
| Company | empresas | tenant root |
| User | usuarios | inclui funcionários |
| Product | produtos | |
| Category | categorias | |
| GradeDimension | cat_grade | |
| GradeItem | itens_grade | estoque por variação |
| StockMovement | entradas / saidas | tipo via motivo |
| GradeMovementDetail | detalhes_grade | polimorfismo por `tipo` |
| CashRegister | caixas | terminal físico |
| CashSession | caixa | sessão PDV |
| Sale | receber (tipo=Venda) | header venda |
| SaleLine | itens_venda | |
| CashWithdrawal | sangrias | |
| Exchange | trocas | |
| Receivable | receber | tipos múltiplos |
| Payable | pagar | |
| Commission | comissoes | |
| Customer | clientes | |
| Supplier | fornecedores | |
| PaymentMethod | forma_pgtos | |
| Frequency | frequencias | |
| Permission | acessos | catálogo |
| PermissionGrant | usuarios_permissoes | |
| Role | cargos | label; nivel em usuarios.nivel |
| Contract | contratos | |
| Attachment | arquivos | |
| PlatformConfig | config | empresa=0 ou por tenant |

## Tabelas de referência / junção

| Tabela | Papel |
| ------ | ----- |
| grupo_acessos | Agrupa itens menu RBAC |
| acessos | Catálogo permissões (35 itens seed) |

## Índices e integridade [observado no SQL]

- PKs auto-increment em todas as tabelas
- FKs implícitas por convenção (sem CONSTRAINT no dump em vários casos)
- Unicidade de `produtos.codigo` enforced apenas na aplicação PHP

## Campos calculados (não persistidos como generated)

| Cálculo | Onde |
| ------- | ---- |
| valor_quebra caixa | fechar-caixa.php |
| total_comissao | buscar-codigo.php |
| lucro % produto | comprar.php |
| estoque baixo count | mensagem.php, estoque/listar.php |
| troco PDV | buscar-codigo.php |

## Observações para reimplementação

1. **`receber` sobrecarregada** — separar `Sale`, `ReceivableAccount`, `PlatformInvoice` em entities distintas com tabela ou STI explícito.
2. **`detalhes_grade`** — tratar como value object collection ou entity filha de StockMovement/SaleLine.
3. **Carrinho PDV** — `itens_venda.venda=0` é state machine; usar aggregate `OpenCart` ou embutir em `CashSession`.
4. **Tenant isolation** — replicar filtro `empresa` como Row-Level Security ou discriminator em ORM.
5. **Auditoria ausente** — created_by/updated_at inconsistentes; considerar na nova modelagem.
