# Modelo Estratégico — Loja PHP (Sistema de Vendas SaaS)

**Baseado em**: `docs/discovery/ecommerce-legado-php/requirements.md` + `ddd-analysis.md`  
**Data da modelagem**: 2026-05-22  
**Nota**: o discovery foi gerado em `docs/discovery/ecommerce-legado-php/` (equivalente ao alias `loja-php` solicitado).

---

## Domínio

| Aspecto | Descrição |
| ------- | --------- |
| **Nome** | **RetailOps SaaS** — Gestão de Vendas e PDV Multi-tenant |
| **Propósito** | Permitir que lojas físicas operem PDV, estoque, financeiro e equipe; e que um operador SaaS gerencie tenants, cobrança e contratos. |
| **Escopo dentro** | Login multi-tenant, PDV/caixa, catálogo com grades, estoque, trocas, contas a receber/pagar, comissões, relatórios PDF, cobrança SaaS, notificações WhatsApp. |
| **Escopo fora** | E-commerce B2C (vitrine/checkout web), gateway de cartão (stub no legado), ERP fiscal completo (NF-e), WMS avançado. |

---

## Subdomínios

| # | Subdomínio | Tipo | Responsabilidade | Complexidade | Estratégia |
| - | ---------- | ---- | ---------------- | ------------ | ---------- |
| SD-01 | **Operação de PDV** | **Core** | Venda presencial: caixa, carrinho, pagamento, comprovante, comissão | Alta | Build |
| SD-02 | **Catálogo e Estoque** | **Core** | Produtos, categorias, grades, movimentação, compras de mercadoria | Alta | Build |
| SD-03 | **Gestão SaaS** | **Core** (plataforma) | Tenants, trial, mensalidade, contratos, bloqueio por inadimplência | Média | Build |
| SD-04 | **Financeiro da Loja** | Supporting | Contas a receber/pagar, fluxo de caixa, baixa de comissões | Média | Build |
| SD-05 | **Trocas e Devoluções** | Supporting | Troca produto entrada/saída com ajuste de estoque | Baixa | Build |
| SD-06 | **Cadastro de Partes** | Supporting | Clientes, fornecedores, funcionários como usuários | Baixa | Build |
| SD-07 | **Configuração da Loja** | Supporting | Formas de pagamento, caixas físicos, parâmetros (desconto, comissão, relatório) | Baixa | Build |
| SD-08 | **Identidade e Acesso** | Generic | Autenticação, RBAC, cargos, autorização gerente | Média | Build (padrão interno) |
| SD-09 | **Relatórios e BI** | Generic | Projeções read-only e PDF | Baixa | Build (CQRS read side) |
| SD-10 | **Notificações** | Generic | WhatsApp diário (contas, estoque, cobrança) | Baixa | Buy (API enviame) + adapter |

---

## Bounded Contexts

| BC | Subdomínio(s) | Cardinalidade | Responsabilidade | Módulo sugerido |
| -- | ------------- | ------------- | ---------------- | --------------- |
| **Platform** | Gestão SaaS | 1:1 | Lifecycle tenant, billing plataforma, contratos, config global | `platform` |
| **Identity & Access** | Identidade e Acesso | 1:1 | Auth, users, permissions, manager PIN | `auth` |
| **Store Settings** | Configuração da Loja | 1:1 | Payment methods, cash register terminals, tenant branding/config | `settings` |
| **Catalog & Inventory** | Catálogo e Estoque | 1:1 | Product, category, grade, stock movements, purchasing | `catalog` |
| **Sales (PDV)** | Operação de PDV | 1:1 | Cash session, cart, sale finalization, sangria | `sales` |
| **Returns** | Trocas e Devoluções | 1:1 | Product exchange between SKUs | `returns` |
| **Finance** | Financeiro da Loja + cobrança tenant (lado contábil) | 1:N | Receivables, payables, commissions, cash flow | `finance` |
| **CRM** | Cadastro de Partes | 1:1 | Customers, suppliers | `crm` |
| **Reporting** | Relatórios e BI | 1:1 | Read models + PDF export | `reporting` |
| **Notifications** | Notificações | 1:1 | WhatsApp digest e agendamento | `notifications` |

**Decisão 1:N (Finance)**: o subdomínio Financeiro absorve tanto contas da loja quanto a representação contábil de vendas/comissões geradas pelo PDV. No legado, `receber`/`pagar` servem ambos os papéis — separar **internamente** por aggregate type, mantendo **um BC** na v1.

---

## Context Map (relações tipadas)

```
┌────────────────────────────────────────────────────────────────────────────┐
│                         CONTEXT MAP — RetailOps SaaS                        │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌──────────────┐                                                          │
│   │  Platform    │──── Customer/Supplier ────▶ Identity & Access            │
│   │   (Core)     │     (provisiona admin)                                    │
│   └──────┬───────┘                                                          │
│          │ Upstream                                                          │
│          │ [Published Language: TenantInvoice, TenantSuspended]              │
│          ▼                                                                   │
│   ┌──────────────┐         Shared Kernel          ┌──────────────┐          │
│   │   Finance    │◀──── TenantId, Money ────────▶│Store Settings│          │
│   │ (Supporting) │                                │ (Supporting) │          │
│   └──────┬───────┘                                └──────┬───────┘          │
│          ▲                                               │                   │
│          │ Downstream                                     │ Shared Kernel     │
│          │ [ACL interna: SaleSettlement]                 │ (PaymentMethod)   │
│          │                                                ▼                   │
│   ┌──────┴───────┐    Upstream [OHS: ProductCatalog]  ┌──────────────┐      │
│   │ Sales (PDV)  │◀───────────────────────────────────│Catalog &     │      │
│   │   (Core)     │    ProductSnapshot, StockLevel    │ Inventory    │      │
│   └──────┬───────┘                                    │   (Core)     │      │
│          │ Downstream                                   └──────┬───────┘      │
│          │ [Published Language: SaleCompleted]                 │             │
│          │                                                     │ Upstream     │
│   ┌──────┴───────┐    Downstream                              │             │
│   │     CRM      │◀───────────────────────────────────────────┤             │
│   │ (Supporting) │    CustomerRef (fiado/troca)               │             │
│   └──────────────┘                                              │             │
│                                                                 │             │
│   ┌──────────────┐    Downstream ◀── StockAdjustmentCommand ──┤             │
│   │   Returns    │─────────────────────────────────────────────┘             │
│   │ (Supporting) │    (1:1 troca ↔ ±estoque)                                  │
│   └──────────────┘                                                          │
│                                                                             │
│   ┌──────────────┐    Conformist (read-only)    ┌──────────────┐           │
│   │  Reporting   │◀══════════════════════════════│ Sales,       │           │
│   │  (Generic)   │◀══════════════════════════════│ Finance,     │           │
│   └──────────────┘                               │ Catalog, CRM │           │
│                                                  └──────────────┘           │
│   ┌──────────────┐    Conformist + Anti-Corruption Layer (adapter HTTP)    │
│   │Notifications │◀── queries agregadas de Finance, Catalog, Platform      │
│   │  (Generic)   │──── Open Host Service externo: api.enviame.com.br       │
│   └──────────────┘                                                          │
│                                                                             │
│   ┌──────────────┐                                                          │
│   │Identity &    │──── Shared Kernel: TenantId, UserId, PermissionKey ──▶   │
│   │Access        │     todos os BCs tenant-scoped                           │
│   └──────────────┘                                                          │
│                                                                             │
└────────────────────────────────────────────────────────────────────────────┘
```

### Tabela de relações

| Upstream | Downstream | Padrão | Contrato / Observação |
| -------- | ---------- | ------ | --------------------- |
| Platform | Identity & Access | Customer/Supplier | Platform cria tenant + admin inicial |
| Platform | Finance | Upstream + Published Language | `TenantInvoice`, vencimento, bloqueio |
| Platform | Sales (indireto) | Upstream | Tenant suspenso impede operação [via Identity] |
| Catalog & Inventory | Sales | Upstream + OHS | API estável: produto por código, preço, estoque |
| Sales | Finance | Downstream + Published Language | Evento `SaleCompleted` → Receivable + Commission |
| Sales | CRM | Downstream | `CustomerId` obrigatório em venda fiada |
| Catalog & Inventory | Returns | Upstream | Comando `AdjustStock` (+1/-1) |
| CRM | Returns | Upstream | `FindOrCreateCustomerByCpf` |
| Catalog & Inventory | Finance | Downstream | Compra gera `Payable(Compra)` |
| Sales, Finance, Catalog, CRM | Reporting | Conformist | Read models; Reporting não altera domínio |
| Finance, Catalog, Platform | Notifications | Conformist | Job lê projeções; ACL isola API WhatsApp |
| Identity & Access | Todos (tenant) | Shared Kernel | `TenantId`, `UserId`, autorização |
| Store Settings | Sales | Shared Kernel | `PaymentMethod`, `DiscountPolicy`, comissão padrão |

---

## Linguagem Ubíqua

### BC Platform

| Termo | Significado neste contexto |
| ----- | -------------------------- |
| **Tenant** | Empresa cliente do SaaS (`empresas`); unidade de isolamento de dados |
| **Trial** | Período de teste com `data_pgto` futura; expiração desativa tenant |
| **Mensalidade** | Valor recorrente cobrado do tenant (`empresas.valor`) |
| **Contrato de Locação** | Documento HTML de licenciamento do software |
| **Bloqueio por Inadimplência** | Suspensão após N dias (`dias_bloqueio`) sem pagamento da fatura SaaS |
| **Operador SAS** | Usuário `nivel=SAS` da plataforma (`empresa=0`) |

### BC Identity & Access

| Termo | Significado neste contexto |
| ----- | -------------------------- |
| **Usuário** | Pessoa autenticada vinculada a um tenant (ou SAS) |
| **Nível** | Papel coarse-grained: SAS, Administrador, Gerente, Operador, Vendedor |
| **Permissão** | Acesso a feature via chave (`acessos.chave`, ex.: `produtos`, `abertura`) |
| **Cargo** | Label organizacional customizável por tenant |
| **Autorização de Gerente** | Credencial adicional para abrir/fechar caixa (no legado: senha plaintext) |

### BC Store Settings

| Termo | Significado neste contexto |
| ----- | -------------------------- |
| **Configuração da Loja** | Parâmetros do tenant: nome, logo, CNPJ, tipo desconto, comissão padrão |
| **Forma de Pagamento** | Meio aceito no PDV com acréscimo percentual opcional (parcelamento) |
| **Caixa Físico** | Terminal PDV cadastrado (`caixas`); distinto de sessão operacional |
| **Token WhatsApp** | Credencial opcional por tenant para notificações |

### BC Catalog & Inventory

| Termo | Significado neste contexto |
| ----- | -------------------------- |
| **Produto** | Item vendável com código de barras, preço e estoque agregado |
| **Categoria** | Agrupamento navegacional; pode estar inativa |
| **Grade** | Dimensão de variação (Cor, Tamanho) com itens e estoque próprio |
| **Estoque** | Quantidade disponível (produto ou combinação de grade) |
| **Nível de Estoque** | Limite mínimo para alerta de reposição |
| **Entrada / Saída** | Movimentação manual com motivo auditável |
| **Compra de Mercadoria** | Entrada de estoque com custo, fornecedor e conta a pagar |
| **Preço Aberto** | Produto com `valor_venda=0`; preço definido no PDV |
| **Detalhe de Grade** | Registro de movimentação por variação (Compra, Venda, Troca) |

### BC Sales (PDV)

| Termo | Significado neste contexto |
| ----- | -------------------------- |
| **Sessão de Caixa** | Período operacional Aberto→Fechado de um operador (`caixa`) |
| **Carrinho** | Itens pendentes (`itens_venda` com `venda=0`) antes da finalização |
| **Venda** | Transação concluída com total, pagamento e itens vinculados |
| **Item de Venda** | Linha: produto × quantidade × valor unitário |
| **Fiado** | Venda a prazo; exige cliente; conta fica em aberto |
| **Desconto** | Redução % ou R$ sobre total (conforme config da loja) |
| **Acréscimo** | Percentual da forma de pagamento sobre total |
| **Troco** | Diferença valor recebido − total (≥ 0) |
| **Sangria** | Retirada de dinheiro do caixa durante sessão |
| **Quebra de Caixa** | Diferença entre valor contado e valor esperado no fechamento |
| **Garantia** | Dias de garantia registrados na venda |
| **Comprovante** | PDF/recibo pós-venda |

> **Homônimos**: "Caixa" no Sales = sessão operacional; em Store Settings = terminal cadastrado.

### BC Returns

| Termo | Significado neste contexto |
| ----- | -------------------------- |
| **Troca** | Operação 1:1: cliente devolve produto A e recebe produto B |
| **Produto de Entrada** | SKU devolvido à loja (+estoque) |
| **Produto de Saída** | SKU entregue ao cliente (−estoque) |

### BC Finance

| Termo | Significado neste contexto |
| ----- | -------------------------- |
| **Conta a Receber** | Direito de cobrança (Venda, manual, ou fatura SaaS) |
| **Conta a Pagar** | Obrigação (Compra, despesa, pagamento comissão) |
| **Baixa** | Quitação: registra `data_pgto` e usuário |
| **Comissão** | Valor devido ao vendedor sobre venda (`total_sem_taxa × %`) |
| **Fluxo de Caixa** | Visão consolidada recebimentos − pagamentos por período |
| **Frequência** | Recorrência em dias (Nenhuma, Diária, Semanal, Mensal) |
| **Anexo** | Arquivo vinculado a conta ou parte (PDF, imagem, doc) |

> **Homônimos**: "Venda" em Sales = transação PDV; em Finance = tipo de `Conta a Receber` originada do PDV.

### BC CRM

| Termo | Significado neste contexto |
| ----- | -------------------------- |
| **Cliente** | Comprador da loja; identificado preferencialmente por CPF |
| **Fornecedor** | Parte fornecedora de mercadoria (PF ou PJ) |
| **Funcionário** | Usuário operacional criado a partir de cadastro de pessoas |

### BC Reporting

| Termo | Significado neste contexto |
| ----- | -------------------------- |
| **Demonstrativo de Lucro** | Relatório receitas − custos/despesas |
| **Relatório de Estoque Baixo** | Produtos abaixo do nível configurado |
| **Período de Análise** | Intervalo data início/fim dos filtros |

### BC Notifications

| Termo | Significado neste contexto |
| ----- | -------------------------- |
| **Digest Diário** | Mensagem WhatsApp única por dia com resumo operacional |
| **Agendamento** | Envio programado via API externa |

---

## Glossário transversal (Shared Kernel)

| Termo | Significado |
| ----- | ----------- |
| **TenantId** | Identificador da empresa (`empresa`); filtro obrigatório em dados de loja |
| **Money** | Valor monetário BRL com 2 decimais; sem negativos em totais de venda |
| **ActiveStatus** | Sim/Não — entidade habilitada para uso |
| **PaymentStatus** | Sim/Não — conta quitada ou em aberto |
