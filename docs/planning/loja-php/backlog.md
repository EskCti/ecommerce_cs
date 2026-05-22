# Backlog — RetailOps (Loja PHP)

**Baseado em**:
- `docs/modeling/loja-php/ddd-strategic-model.md`
- `docs/modeling/loja-php/ddd-tactical-model.md`
- `docs/discovery/ecommerce-legado-php/requirements.md`
- `docs/migration/loja-php/migration-strategy.md`

**Data**: 2026-05-22  
**Stack**: **C#** (ASP.NET Core 8 + EF Core) · **Vue 3 + PrimeVue** (web admin) · **Kotlin Android** (PDV mobile)  
**Total**: 12 épicos (2 técnicos + 10 BCs), 38 stories, ~420 tasks estimadas

---

## Roadmap

### Release 0 — Bootstrap

- EP-000 [TECH]: Projeto, shared kernel, EF Core, Docker, CI/CD, shell Vue

### Release 1 — MVP (PDV + tenant mínimo)

- EP-001: Identity & Access
- EP-002: Platform (trial + SAS básico)
- EP-003: Store Settings (formas pgto, caixas — prerequisite PDV)
- EP-005: Catalog & Inventory
- EP-006: Sales (PDV) — **parallel run com legado PHP**

### Release 2 — Operação completa

- EP-004: CRM
- EP-007: Finance
- EP-008: Returns

### Release 3 — Observabilidade e comunicação

- EP-009: Reporting
- EP-010: Notifications

### Release 4 — Cutover

- EP-011 [TECH]: Desligamento PHP, remoção ACL, ETL final

---

## Template — Tasks full-stack por feature

Reutilizar ao final de cada US com frontend/mobile. Substituir `<X>`, `<xs>`, `<Endpoint>`.

```markdown
- [ ] `interface:entity` Entidade frontend <X> (~1h)
  - **Agent:** `Frontend Entity (Vue)`
  - **Prompt:** "Crie entidade <X> TypeScript pura com Result<T>, validações espelhando domínio."
- [ ] `interface:usecase` Use cases frontend (~2h)
  - **Agent:** `Frontend UseCase (Vue)`
  - **Prompt:** "Create<X>UseCase e List<X>sUseCase com Promise<Result<T>>."
- [ ] `interface:repository` HttpRepository (~2h)
  - **Agent:** `Frontend Repository (Vue)`
  - **Prompt:** "Implemente I<X>Repository consumindo <Endpoint>; map DTO→entity."
- [ ] `interface:page` Listagem PrimeVue DataTable (~2h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "Página listagem <xs> com PrimeVue DataTable, filtros, paginação."
- [ ] `interface:form-web` Formulário vee-validate (~2h)
  - **Agent:** `Frontend Form (Vue)`
  - **Prompt:** "Form cadastro/edição <X> com vee-validate + PrimeVue inputs."
- [ ] `interface:mobile-entity` Entidade Android <X> (~1h)
  - **Agent:** `Mobile Entity (Android)`
  - **Prompt:** "data class <X> Kotlin puro + sealed Result."
- [ ] `interface:mobile-usecase` Use cases Android (~2h)
  - **Agent:** `Mobile UseCase (Android)`
  - **Prompt:** "Create<X>UseCase suspend + List<X>sUseCase."
- [ ] `interface:mobile-repository` Retrofit (~2h)
  - **Agent:** `Mobile Repository (Android)`
  - **Prompt:** "I<X>Repository + Retrofit adapter, map DTO→domain."
- [ ] `interface:mobile` Tela Compose (~3h)
  - **Agent:** `Mobile Screen (Android)`
  - **Prompt:** "LazyColumn listagem <xs> + ViewModel StateFlow."
- [ ] `interface:mobile-form` Form Compose (~2h)
  - **Agent:** `Mobile Form (Android)`
  - **Prompt:** "OutlinedTextField + validação; submit via ViewModel."
- [ ] `test:unit` Testes domain + application (~2h)
  - **Agent:** `Unit Tests (C#)`
  - **Prompt:** "xUnit + Moq: VOs, entity, use case. Fluxo feliz + erros."
- [ ] `test:coverage` Cobertura ≥95% domain+app (~30min)
  - **Agent:** `Unit Tests (C#)`
  - **Prompt:** "Coverlet ≥95% em *.Core e *.Application."
- [ ] `test:e2e` E2E API (~2h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "WebApplicationFactory: fluxo HTTP principal da US."
```

---

## EP-000 [TECH]: Bootstrap e Infraestrutura

**Descrição**: Monorepo RetailOps — backend C#, web Vue, mobile Android, Docker prod, CI/CD, EF Core, multi-tenant  
**Tamanho**: M  
**Dependências**: nenhuma  
**Release**: 0  
**OpenSpec**: `bootstrap-retailops`

### US-000: Setup do projeto full-stack

> Como desenvolvedor, quero o projeto configurado com Docker e CI/CD, para implementar BCs com entrega contínua desde o início.

**Prioridade**: Must | **Estimativa**: 8 | **Ref**: RNF-001, RNF-006

**Critérios de Aceitação**:

- [ ] Solução .sln compila; API responde `/health`
- [ ] Vue admin roda com shell sidebar/topbar
- [ ] Android app compila e aponta para API local
- [ ] `docker compose up` sobe API + MariaDB/Postgres + web
- [ ] CI executa test + build Docker em PR

**Tasks**:

- [ ] `infra:fullstack` Orquestrar bootstrap RetailOps (~1h)
  - **Agent:** `Config Project Full-Stack`
  - **Prompt:** "Bootstrap RetailOps: backend C# ASP.NET Core, frontend Vue 3 PrimeVue (apps/web-vue), mobile Android Kotlin Compose (apps/mobile-android). Incluir docker e cicd. OpenSpec: bootstrap-retailops."
- [ ] `infra:setup` Solução C# multi-projeto (~2h)
  - **Agent:** `Config Project (C#)`
  - **Prompt:** "Crie solução RetailOps.sln: RetailOps.Shared, RetailOps.Api, estrutura Core/Application/Infrastructure por BC vazia. Clean Architecture."
- [ ] `domain:shared` Shared kernel C# (~2h)
  - **Agent:** `Config Shared Core (C#)`
  - **Prompt:** "Entity base, Result<T>, TenantId, Money, Email, IUseCase, IRepository, TransactionManager."
- [ ] `infra:db` EF Core + docker-compose dev (~2h)
  - **Agent:** `Config EF Core (C#)`
  - **Prompt:** "DbContext base, migrations vazias, ConnectionString, docker-compose com MariaDB compatível schema sas."
- [ ] `infra:docker` Dockerfiles produção (~1h)
  - **Agent:** `Config Docker (C#)`
  - **Prompt:** "Multi-stage Dockerfile API + docker-compose.prod.yml."
- [ ] `infra:cicd` GitHub Actions (~2h)
  - **Agent:** `Config CI/CD (C#)`
  - **Prompt:** "CI: dotnet test + coverlet ≥95% domain/app. CD: build/push Docker main."
- [ ] `infra:shell-web` Shell Vue PrimeVue (~2h)
  - **Agent:** `Config Shared Web (Vue)`
  - **Prompt:** "Shell admin: sidebar colapsável, topbar, rodapé, rotas tenant + platform."
- [ ] `infra:setup` Bootstrap Android (~2h)
  - **Agent:** `Config Project (Android)`
  - **Prompt:** "App Kotlin Compose + Hilt + Retrofit + go_router; base URL configurável."
- [ ] `infra:migration` ACL base LegacySasDbContext (~2h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "RetailOps.Infrastructure.Legacy: DbContext mapeando tabelas sas.sql; sem vazar nomes legado ao domínio."
- [ ] `test:e2e` Smoke test health (~1h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "IntegrationTests: GET /health retorna 200."

### US-001: Multi-tenancy e feature flags de migração

> Como operador, quero isolamento por tenant e flags de cutover, para migrar do PHP incrementalmente.

**Prioridade**: Must | **Estimativa**: 5 | **Ref**: RNF-001, migration-strategy

**Critérios de Aceitação**:

- [ ] Middleware extrai TenantId (JWT/header)
- [ ] Queries filtradas por tenant automaticamente
- [ ] `Migration:PilotTenantIds` controla roteamento por BC

**Tasks**:

- [ ] `domain:vo` TenantId VO (~1h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "TenantId value object; 0 = platform scope."
- [ ] `app:usecase` TenantContext middleware (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "ITenantContext + middleware ASP.NET Core; integrar com JWT claims."
- [ ] `interface:controller` Config endpoint Migration (~1h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "GET /api/admin/migration-status (SAS only)."

---

## EP-001: Identity & Access

**Bounded Context**: BC Identity & Access  
**Tamanho**: M | **Dependências**: EP-000 | **Release**: 1

### US-010: Login JWT (email/CPF + senha)

> Como usuário, quero autenticar com email ou CPF e senha, para acessar painel tenant ou SAS.

**Prioridade**: Must | **Estimativa**: 8 | **Ref**: RF-001, RN-001–RN-004

**Critérios de Aceitação**:

- [ ] POST `/api/auth/login` retorna JWT
- [ ] Usuário inativo ou trial expirado → 403 com mensagem adequada
- [ ] Não-admin sem permissões → 403
- [ ] ACL MD5 legado + re-hash bcrypt no primeiro login

**Tasks**:

- [ ] `infra:auth` Bootstrap auth core + backend (~3h)
  - **Agent:** `Config Auth Core (C#)` + `Config Auth Backend Basic (C#)`
  - **Prompt:** "Auth full RBAC: User, Permission, JWT, ASP.NET Identity adaptado. Endpoints register/login/me. Compatível multi-tenant."
- [ ] `domain:vo` Email, Cpf, PasswordHash, UserLevel (~2h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "VOs com Create() Result<T>. UserLevel enum: Sas, Administrador, Gerente, Operador, Vendedor."
- [ ] `domain:entity` User aggregate + PermissionGrant (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "User AR com AssignPermission(), Deactivate(). PermissionGrant entity filha."
- [ ] `domain:service` AuthorizationPolicy, LegacyMd5Verifier (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Admin/SAS bypass; RequiresGrants(); ILegacyMd5PasswordVerifier."
- [ ] `app:usecase` AuthenticateUserUseCase (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Login email/CPF; ACL usuarios; emite JWT; dispara PasswordRehashRequested."
- [ ] `infra:persistence` UserEfRepository + LegacyUserAdapter (~3h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "UserEfRepository + LegacyUserMapper conforme acl-design.md."
- [ ] `interface:controller` AuthController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "POST /api/auth/login, POST /api/auth/register (trial), GET /api/auth/me."
- [ ] `interface:page` Tela login Vue (~2h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "Página pública login; armazena JWT; redireciona tenant vs SAS por level."
- [ ] `test:unit` + `test:e2e` Auth (~3h)
  - **Agent:** `Unit Tests (C#)` + `E2E Tests (C#)`
  - **Prompt:** "Testes login válido/inválido/trial expirado/sem permissão."

### US-011: RBAC — permissões por usuário

> Como administrador, quero atribuir permissões granulares, para controlar acesso ao menu.

**Prioridade**: Must | **Estimativa**: 5 | **Ref**: RF-006, RF-007

**Critérios de Aceitação**:

- [ ] CRUD permissões por usuário
- [ ] Catálogo `acessos` seed (35 itens legado)
- [ ] Guard Vue bloqueia rotas por PermissionKey

**Tasks**:

- [ ] `app:usecase` AssignPermissionsUseCase, ListPermissionsQuery (~2h)
  - **Agent:** `Core Use Case (C#)` + `Core Query CQRS (C#)`
- [ ] `interface:controller` UsersPermissionsController (~2h)
  - **Agent:** `Backend Controller (C#)`
- [ ] `interface:page` + `interface:form-web` Gestão usuários/permissões Vue (~4h)
  - **Agent:** `Frontend Page (Vue)` + `Frontend Form (Vue)`
- [ ] Aplicar **Template full-stack** para User (Android: tela perfil apenas) (~8h)

### US-012: Autorização de gerente (PIN caixa)

> Como gerente, quero validar PIN para abrir/fechar caixa, substituindo senha plaintext legada.

**Prioridade**: Must | **Estimativa**: 3 | **Ref**: RF-050, RF-056, RNF-004

**Tasks**:

- [ ] `domain:vo` ManagerPin (~1h) — **Agent:** `Core Value Object (C#)`
- [ ] `domain:service` ManagerAuthenticationService (~2h) — **Agent:** `Core Domain Service (C#)`
- [ ] `app:usecase` VerifyManagerPinUseCase (~1h) — **Agent:** `Core Use Case (C#)`
- [ ] `interface:controller` POST /api/auth/verify-manager-pin (~1h) — **Agent:** `Backend Controller (C#)`

---

## EP-002: Platform (SaaS)

**Bounded Context**: BC Platform | **Tamanho**: M | **Dep.**: EP-001 | **Release**: 1

### US-020: Cadastro trial self-service

> Como prospect, quero registrar trial, para testar o sistema por N dias.

**Prioridade**: Must | **Estimativa**: 5 | **Ref**: RF-002, RN-003

**Critérios de Aceitação**:

- [ ] Cria Company (teste=Sim) + admin user
- [ ] `data_pgto = hoje + dias_teste`
- [ ] Email único

**Tasks**:

- [ ] `domain:entity` Company, PlatformConfig (~2h) — **Agent:** `Core Entity (C#)`
- [ ] `domain:service` TrialExpirationPolicy (~2h) — **Agent:** `Core Domain Service (C#)`
- [ ] `app:usecase` RegisterTrialCompanyUseCase (~2h) — **Agent:** `Core Use Case (C#)`
- [ ] `infra:persistence` LegacyCompanyAdapter (~2h) — **Agent:** `Backend Data (C#)`
- [ ] `interface:controller` POST /api/platform/trial (~1h) — **Agent:** `Backend Controller (C#)`
- [ ] `interface:form-web` Modal cadastro trial Vue (~2h) — **Agent:** `Frontend Form (Vue)`

### US-021: Gestão SAS — empresas e contratos

> Como operador SAS, quero CRUD empresas e contratos, para gerenciar tenants.

**Prioridade**: Must | **Estimativa**: 8 | **Ref**: RF-010, RF-016

**Tasks**:

- [ ] `infra:setup` Módulo Platform (~1h) — **Agent:** `Config New Module (C#)`
- [ ] `domain:entity` Company, Contract (~2h) — **Agent:** `Core Entity (C#)`
- [ ] `app:usecase` CreateCompany, UpdateCompany, SaveContract (~4h) — **Agent:** `Core Use Case (C#)`
- [ ] `app:query` ListCompaniesQuery (~1h) — **Agent:** `Core Query CQRS (C#)`
- [ ] `interface:controller` PlatformCompaniesController (~2h) — **Agent:** `Backend Controller (C#)`
- [ ] **Template full-stack** Company (Vue SAS panel; sem mobile) (~12h)

### US-022: Cobrança e bloqueio por inadimplência

> Como operador SAS, quero cobrar mensalidade e bloquear tenant inadimplente.

**Prioridade**: Should | **Estimativa**: 5 | **Ref**: RF-014, RN-010, RN-011

**Tasks**:

- [ ] `domain:service` TenantBillingPolicy, TenantSuspensionPolicy (~3h)
- [ ] `app:usecase` IssueTenantInvoice, SuspendOverdueTenant (~3h)
- [ ] Event handler TenantSuspended → deactivate users (~2h)

---

## EP-003: Store Settings

**BC**: Store Settings | **Tamanho**: P | **Dep.**: EP-001 | **Release**: 1

### US-030: Configuração da loja

> Como administrador tenant, quero configurar nome, logo, desconto e comissão padrão.

**Prioridade**: Must | **Estimativa**: 5 | **Ref**: RF-027

**Tasks**: módulo Settings + StoreConfig aggregate + CRUD API + form Vue (~16h, agents CS/Vue padrão)

### US-031: Formas de pagamento e caixas físicos

> Como administrador, quero cadastrar formas de pagamento e terminais de caixa.

**Prioridade**: Must | **Estimativa**: 5 | **Ref**: RF-025, RF-026, RN-047

**Tasks**: PaymentMethod + CashRegisterTerminal aggregates + list/form Vue (~14h)

---

## EP-004: CRM

**BC**: CRM | **Tamanho**: P | **Dep.**: EP-001 | **Release**: 2

### US-040: CRUD clientes

> Como operador, quero cadastrar clientes, para vendas fiado e histórico.

**Prioridade**: Must | **Estimativa**: 5 | **Ref**: RF-020

**Tasks**: Customer aggregate + FindOrCreateByCpf + full-stack template (~20h)

### US-041: CRUD fornecedores

> Como comprador, quero cadastrar fornecedores, para compras de mercadoria.

**Prioridade**: Should | **Estimativa**: 3 | **Ref**: RF-021

**Tasks**: Supplier aggregate + CRUD (~16h)

---

## EP-005: Catalog & Inventory

**BC**: Catalog & Inventory | **Tamanho**: G | **Dep.**: EP-003 | **Release**: 1

### US-050: CRUD produtos e categorias

> Como gestor, quero cadastrar produtos com código de barras e categorias.

**Prioridade**: Must | **Estimativa**: 8 | **Ref**: RF-030–RF-032, RN-020–RN-022

**Critérios de Aceitação**:

- [ ] Código único por tenant
- [ ] Preço aberto (valor 0)
- [ ] Upload foto com whitelist

**Tasks**:

- [ ] `infra:setup` Módulo Catalog (~1h) — **Agent:** `Config New Module (C#)`
- [ ] `domain:vo` Barcode, SalePrice, StockQuantity, ProfitMargin (~2h)
- [ ] `domain:entity` Product, Category (~3h)
- [ ] `domain:repository` IProductRepository, ICategoryRepository (~1h)
- [ ] `app:usecase` CreateProduct, UpdateProduct, ListProducts (~4h)
- [ ] `infra:persistence` ProductEfRepository + LegacyProductAdapter (~4h)
- [ ] `interface:controller` CatalogProductsController (~2h)
- [ ] **Template full-stack** Product (~18h)
- [ ] `test:unit` + `test:e2e` + parallel run estoque read (~4h)

### US-051: Grades (Cor/Tamanho)

> Como gestor, quero variações com estoque por combinação.

**Prioridade**: Must | **Estimativa**: 8 | **Ref**: RF-033, RF-034

**Tasks**: GradeDimension, GradeOption + ConfigureProductGradeUseCase + UI grade editor Vue (~20h)

### US-052: Movimentação e compra de estoque

> Como operador, quero entradas/saídas manuais e compra com conta a pagar.

**Prioridade**: Must | **Estimativa**: 8 | **Ref**: RF-040–RF-042, RN-030–RN-031

**Tasks**: StockMovement, PurchaseStockUseCase, ProductPurchased event → Finance (~22h)

### US-053: Alerta estoque baixo

> Como gestor, quero ver produtos abaixo do nível mínimo.

**Prioridade**: Should | **Estimativa**: 3 | **Ref**: RF-044, RN-021

**Tasks**: LowStockPolicy + ListLowStockProductsQuery + dashboard Vue (~8h)

---

## EP-006: Sales (PDV)

**BC**: Sales | **Tamanho**: GG | **Dep.**: EP-003, EP-005 | **Release**: 1

### US-060: Abrir sessão de caixa

> Como operador, quero abrir caixa com fundo inicial validado pelo gerente.

**Prioridade**: Must | **Estimativa**: 5 | **Ref**: RF-050, RN-040

**Tasks**: CashSession aggregate + OpenCashSessionUseCase + VerifyManagerPin + Vue + **Android tela abertura** (~24h)

### US-061: Carrinho PDV — scan e itens

> Como operador, quero escanear produtos e montar carrinho.

**Prioridade**: Must | **Estimativa**: 8 | **Ref**: RF-051, RF-052, RN-045

**Tasks**: AddItemToCart, ConfirmGradeForItem, CartStockReservation + **Android PDV scan UI** (~30h)

### US-062: Finalizar venda

> Como operador, quero finalizar com pagamento, desconto, fiado e comissão.

**Prioridade**: Must | **Estimativa**: 13 | **Ref**: RF-053, RN-041–RN-046

**Critérios de Aceitação**:

- [ ] Parallel run 2 semanas vs PHP sem divergência >0,1%
- [ ] SaleCompleted → Finance
- [ ] Troco ≥ 0; fiado exige cliente

**Tasks**:

- [ ] `domain:entity` Sale, SaleLine (~3h)
- [ ] `domain:service` SaleFinalizationPolicy, CommissionCalculator (~3h)
- [ ] `app:usecase` FinalizeSaleUseCase (~4h)
- [ ] `infra:persistence` LegacySaleAdapter (ACL crítica) (~6h)
- [ ] `interface:controller` SalesCartController (~3h)
- [ ] Vue PDV checkout + **Android finalize flow** (~20h)
- [ ] `test:e2e` Parallel run logger (~4h)

### US-063: Sangria e fechamento de caixa

> Como gerente, quero sangria e fechamento com cálculo de quebra.

**Prioridade**: Must | **Estimativa**: 8 | **Ref**: RF-055, RF-056, RN-040

**Tasks**: RegisterCashWithdrawal, CloseCashSession + CashSessionClosingPolicy (~22h)

### US-064: Cancelar venda

> Como gerente, quero cancelar venda devolvendo estoque.

**Prioridade**: Must | **Estimativa**: 5 | **Ref**: RF-073, RN-060

**Tasks**: CancelSaleUseCase + handlers SaleCancelled (~14h)

---

## EP-007: Finance

**BC**: Finance | **Tamanho**: G | **Dep.**: EP-006 | **Release**: 2

### US-070: Contas a receber e baixa

> Como financeiro, quero gerenciar recebimentos e baixar contas.

**Prioridade**: Must | **Estimativa**: 8 | **Ref**: RF-070, RN-061

**Tasks**: Receivable aggregate (discriminator) + SettleReceivable + LegacyReceivableMapper (~24h)

### US-071: Contas a pagar e compras

> Como financeiro, quero despesas e compras com anexos.

**Prioridade**: Must | **Estimativa**: 8 | **Ref**: RF-071, RF-072

**Tasks**: Payable + Attachment + handler ProductPurchased (~22h)

### US-072: Comissões e fluxo de caixa

> Como financeiro, quero baixar comissões e ver fluxo consolidado.

**Prioridade**: Must | **Estimativa**: 8 | **Ref**: RF-074, RF-075, RN-046

**Tasks**: Commission + PayCommission + CashFlowAggregator query (~20h)

---

## EP-008: Returns

**BC**: Returns | **Tamanho**: P | **Dep.**: EP-005, EP-004 | **Release**: 2

### US-080: Registrar troca de produtos

> Como operador, quero registrar troca 1:1 com ajuste de estoque.

**Prioridade**: Should | **Estimativa**: 5 | **Ref**: RF-060, RN-050–RN-051

**Tasks**: Exchange aggregate + RegisterExchangeUseCase + form Vue (~18h)

---

## EP-009: Reporting

**BC**: Reporting | **Tamanho**: M | **Dep.**: EP-007 | **Release**: 3

### US-090: Relatórios operacionais (vendas, estoque, caixas)

> Como gestor, quero exportar relatórios PDF filtrados por período.

**Prioridade**: Should | **Estimativa**: 8 | **Ref**: RF-080, RF-082

**Tasks**: ReportSalesQuery, ReportLowStockQuery, ReportCashSessionsQuery + QuestPDF adapter (~20h)

### US-091: Comprovante e demonstrativo de lucro

> Como operador, quero recibo de venda e DRE simplificado.

**Prioridade**: Should | **Estimativa**: 5 | **Ref**: RF-057, RF-080

**Tasks**: GenerateReceiptQuery, ReportProfitQuery (~12h)

---

## EP-010: Notifications

**BC**: Notifications | **Tamanho**: P | **Dep.**: EP-007 | **Release**: 3

### US-100: Digest diário WhatsApp

> Como lojista, quero resumo diário de contas e estoque baixo.

**Prioridade**: Could | **Estimativa**: 5 | **Ref**: RF-090, RN-070–RN-071

**Tasks**: SendDailyTenantDigestHandler + EnviameWhatsAppAdapter + hosted service (~14h)

---

## EP-011 [TECH]: Cutover e desligamento PHP

**Tamanho**: M | **Dep.**: EP-001–EP-010 | **Release**: 4

### US-110: Desligamento do legado

> Como operador, quero 100% tráfego no C# sem PHP em produção.

**Prioridade**: Must | **Estimativa**: 8 | **Ref**: migration-strategy Fase 11

**Critérios de Aceitação**:

- [ ] Zero tenants com flag PHP
- [ ] ACL Legacy removida
- [ ] Backup sas.sql + uploads
- [ ] 30 dias monitoramento sem rollback

**Tasks**:

- [ ] ETL histórico tabelas normalizadas (~8h)
- [ ] Remover YARP routes PHP (~2h)
- [ ] Decommission runbook + backup (~4h)
- [ ] `test:e2e` regressão full suite (~4h)

---

## Dependências entre épicos

```
EP-000 ──▶ EP-001 ──▶ EP-002, EP-003, EP-004
              │
              └──▶ EP-005 ──▶ EP-006 ──▶ EP-007 ──▶ EP-009, EP-010
                        │         │
                        └──▶ EP-008  EP-011 (após todos)
```

---

## Próximos passos

1. `openspec-propose "bootstrap-retailops"` → EP-000
2. `openspec-propose "ep-001-auth"` → EP-001
3. Implementar via `openspec-apply-change` ou agents diretos por task
