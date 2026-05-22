# Estratégia de Migração — Sistema de Vendas PHP → DDD/Clean Architecture (C#)

**Baseado em**:
- `docs/discovery/ecommerce-legado-php/requirements.md`
- `docs/discovery/ecommerce-legado-php/ddd-analysis.md`
- `docs/modeling/loja-php/ddd-strategic-model.md`
- `docs/modeling/loja-php/ddd-tactical-model.md`

**Legado analisado**: `exemplos/php/vendas/` (`sas/`, `sistema/`, `rel/`, `rel_sistema/`) + `exemplos/php/sas.sql`  
**Destino**: ASP.NET Core 8+ monólito modular, EF Core, Clean Architecture por BC  
**Data**: 2026-05-22  
**Padrão escolhido**: **Strangler Fig** + **Parallel Run** nos BCs Core

---

## Diagnóstico do Legado

### Estado atual

| Dimensão | Situação observada | Impacto |
| -------- | ------------------ | ------- |
| **Arquitetura** | PHP procedural; 1 script = 1 ação AJAX; 3 apps (`index`, `sas`, `sistema`) | Extração por BC exige façade/router |
| **Banco** | MariaDB único `sas`; 24 tabelas; multi-tenant via coluna `empresa` | Schema compartilhado; strangler com ACL sobre mesmas tabelas inicialmente |
| **Cobertura de testes** | Ausente [observado] | Alto risco; parallel run + testes E2E obrigatórios |
| **Acoplamento** | `receber` polimórfica (Venda + SaaS + manual); `itens_venda.venda=0` = carrinho | Modelo anêmico; ACL crítica |
| **Segurança** | MD5, SQL concatenado, senha gerente plaintext | Não replicar; cutover auth exige re-hash ou reset |
| **Documentação** | Discovery + modelagem DDD concluídos | Reduz risco de requisitos |
| **Integrações** | WhatsApp (enviame), dompdf; cartão stub | ACL para APIs externas |
| **Tráfego** | SaaS multi-tenant em produção [inferido] | Big Bang descartado |

### Dívida técnica crítica (não migrar “as-is”)

1. Senhas MD5 → bcrypt/Identity na reimplementação C#.
2. Tabela `receber` como header de venda → aggregate `Sale` + `SaleReceivable` separados.
3. Ausência de transações explícitas no PDV → `TransactionManager` + unit of work EF Core.
4. Uploads em filesystem PHP → blob storage ou path abstraído com compatibilidade temporária.

### Risco por Bounded Context

| BC | Risco | Complexidade | Tipo | Tabelas legado principais | Integrações |
| -- | ----- | ------------ | ---- | ------------------------- | ----------- |
| **Identity & Access** | Médio | Média | Generic | `usuarios`, `usuarios_permissoes`, `acessos`, `grupo_acessos`, `cargos` | Sessão PHP coexistência |
| **Platform** | Médio | Média | Core (SAS) | `empresas`, `config`(0), `contratos`, `receber`(tipo Empresa) | Bloqueio tenant |
| **Store Settings** | Baixo | Baixa | Supporting | `config`, `forma_pgtos`, `caixas` | — |
| **CRM** | Baixo | Baixa | Supporting | `clientes`, `fornecedores` | — |
| **Catalog & Inventory** | Alto | Alta | Core | `produtos`, `categorias`, `cat_grade`, `itens_grade`, `entradas`, `saidas`, `detalhes_grade` | Grades 2D |
| **Sales (PDV)** | **Alto** | **Alta** | Core | `caixa`, `caixas`, `itens_venda`, `receber`(Venda), `sangrias`, `comissoes` | Estoque tempo real |
| **Finance** | Alto | Alta | Supporting | `receber`, `pagar`, `comissoes`, `frequencias`, `arquivos` | Vendas + compras |
| **Returns** | Baixo | Baixa | Supporting | `trocas`, `detalhes_grade` | Catalog |
| **Reporting** | Médio | Média | Generic | queries em `rel/`, `rel_sistema/` | dompdf → QuestPDF/iText |
| **Notifications** | Baixo | Baixa | Generic | `api/mensagem.php` | enviame API |

---

## Padrão de Migração

### Escolha: Strangler Fig

**Justificativa**:
- Sistema SaaS com tenants ativos e PDV operacional — indisponibilidade total é inaceitável.
- BCs migráveis incrementalmente com roteamento por URL/API.
- Banco compartilhado permite ACL lendo/escrevendo tabelas legadas até cutover por BC.

**Complemento: Parallel Run** (obrigatório para Sales e Catalog):
- Período em que tenant piloto executa operação no PHP e validação shadow no C#.
- Comparar totais de venda, estoque e comissão antes do cutover.

**Big Bang descartado** — escopo (~10 BCs, PDV crítico, zero testes legado).

### Arquitetura de coexistência

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         STRANGLER FIG — RetailOps                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Browser / Mobile                                                       │
│        │                                                                 │
│        ▼                                                                 │
│   ┌─────────────────┐                                                    │
│   │  YARP / Nginx   │  Feature flag: TenantId ∈ pilotos → API C#       │
│   │  Reverse Proxy  │                                                    │
│   └────────┬────────┘                                                    │
│            │                                                             │
│     ┌──────┴──────┐                                                      │
│     ▼             ▼                                                      │
│ ┌──────────────┐  ┌──────────────────────────────────────────┐          │
│ │ PHP Legado   │  │ ASP.NET Core 8 (Monólito Modular)         │          │
│ │ vendas/      │  │ src/RetailOps.Api                          │          │
│ │  sistema/    │  │ src/RetailOps.Auth.*                       │          │
│ │  sas/        │  │ src/RetailOps.Platform.*                   │          │
│ │  rel_*/      │  │ src/RetailOps.Catalog.* … Sales.* …        │          │
│ └──────┬───────┘  └──────────────────┬───────────────────────┘          │
│        │                             │                                   │
│        └──────────────┬──────────────┘                                   │
│                       ▼                                                  │
│              ┌─────────────────┐                                           │
│              │  MariaDB `sas`  │  Fase 1–3: schema legado + ACL         │
│              │  (compartilhado) │  Fase 4+: migrations EF por BC         │
│              └─────────────────┘                                           │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Stack destino C#

| Camada | Tecnologia | Skill |
| ------ | ---------- | ----- |
| Solução | `.sln` multi-projeto Clean Architecture | `config-project-cs` |
| API | ASP.NET Core 8, Minimal API ou Controllers | `backend-controller-cs` |
| Domínio | Class libraries por BC | `core-entity-cs`, `core-value-object-cs`, … |
| Persistência | EF Core 8 + Fluent API | `config-efcore-cs`, `backend-data-cs` |
| Auth | ASP.NET Core Identity + JWT | `config-auth-backend-basic-cs`, `config-auth-core-full-cs` |
| Testes | xUnit + WebApplicationFactory | `test-unit-cs`, `test-e2e-cs` |
| CI/CD | GitHub Actions | `config-cicd-cs` |
| Container | Docker multi-stage | `config-docker-cs` |

**Estrutura de projetos sugerida**:

```
src/
  RetailOps.Shared/              ← kernel (Result, Entity, TenantId, Money)
  RetailOps.Auth.Core/
  RetailOps.Auth.Application/
  RetailOps.Auth.Infrastructure/
  RetailOps.Platform.Core/
  … (um conjunto Core/Application/Infrastructure por BC)
  RetailOps.Api/                 ← composição, DI, middleware TenantId
tests/
  RetailOps.Auth.UnitTests/
  RetailOps.IntegrationTests/
```

---

## Sequência de Migração

Ordem: **Generic → Supporting (baixo acopl.) → Core (com parallel run) → Supporting (alto acopl.) → Generic (relatórios/notificações)**

| Fase | Sprint(s) | BC / Entrega | Tipo | Risco | Dependências | Skills principais |
| ---- | --------- | ------------ | ---- | ----- | ------------ | ----------------- |
| **0** | 1–2 | Bootstrap solução C#, EF Core, Docker, CI, YARP, feature flags, ACL base | TECH | Baixo | — | `config-project-cs`, `config-efcore-cs`, `config-docker-cs`, `config-cicd-cs`, `config-shared-core-cs` |
| **1** | 2 | **Identity & Access** — JWT, RBAC, login API; ACL `usuarios` | Generic | Médio | F0 | `config-auth-core-full-cs`, `config-auth-backend-basic-cs` |
| **2** | 2 | **Platform** — tenants, trial, contratos; painel SAS API | Core (SAS) | Médio | F1 | `config-new-module-cs` |
| **3** | 1 | **Store Settings** — config loja, formas pgto, caixas cadastro | Supporting | Baixo | F1 | `config-new-module-cs` |
| **4** | 1–2 | **CRM** — clientes, fornecedores | Supporting | Baixo | F1 | `config-new-module-cs` |
| **5** | 2–3 | **Catalog & Inventory** — produtos, grades, movimentação; **parallel run** estoque | Core | Alto | F3, F4 | `core-*-cs`, `backend-data-cs` |
| **6** | 3–4 | **Sales (PDV)** — caixa, carrinho, venda; **parallel run** obrigatório | Core | **Alto** | F5 | `core-*-cs`, `test-e2e-cs` |
| **7** | 2–3 | **Finance** — receber/pagar, comissões, fluxo | Supporting | Alto | F6 | `config-new-module-cs` |
| **8** | 1 | **Returns** — trocas | Supporting | Baixo | F5, F4 | `config-new-module-cs` |
| **9** | 2 | **Reporting** — CQRS read + PDF | Generic | Médio | F6, F7 | `core-query-cqrs-cs` |
| **10** | 1 | **Notifications** — WhatsApp digest | Generic | Baixo | F7 | ACL HTTP enviame |
| **11** | 1 | Desligamento PHP, remoção ACL, arquivamento schema legado | TECH | Médio | F1–F10 | — |

**Estimativa total**: ~18–22 sprints (equipe 2–3 devs), variando com escopo de frontend.

### Tenant piloto (recomendado)

Migrar primeiro **1 tenant de teste** (`empresas.teste=Sim`) com flag `Migration:PilotTenantIds` no `appsettings`. Expandir gradualmente por onda de tenants após parallel run OK.

---

## Plano de Coexistência

### Roteamento (YARP / Nginx)

| Rota legado | Rota nova (C#) | BC | Condição cutover |
| ----------- | -------------- | -- | ---------------- |
| `POST /vendas/autenticar.php` | `POST /api/auth/login` | Auth | Flag `auth_migrated` |
| `GET /vendas/sistema/?pagina=produtos` | `GET /api/catalog/products` | Catalog | Flag por tenant |
| `POST …/pdv/buscar-codigo.php` | `POST /api/sales/cart/*` | Sales | **Parallel run 2 semanas** |
| `GET /vendas/sas/?pagina=empresas` | `GET /api/platform/companies` | Platform | Flag global SAS |

### Feature flags

```json
{
  "Migration": {
    "PilotTenantIds": [15, 17],
    "Modules": {
      "Auth": true,
      "Platform": true,
      "Catalog": false,
      "Sales": false
    }
  }
}
```

Middleware ASP.NET Core + header `X-Tenant-Id` espelha filtro `empresa` do PHP.

### Estratégia de dados

| Fase | Abordagem |
| ---- | --------- |
| **1–3** | **ACL read/write** nas tabelas legado via EF Core (DbSet mapeando nomes atuais) |
| **4–6** | **Dual-write opcional** para Sales durante parallel run (log de divergência) |
| **7+** | **Migrations EF** introduzem tabelas normalizadas; job ETL copia histórico |
| **11** | Views legado arquivadas; drop tabelas obsoletas após backup |

### Autenticação durante transição

1. **Fase híbrida**: login C# valida contra `usuarios.senha_crip` (MD5) via ACL; emite JWT.
2. **Pós-cutover auth**: job re-hash bcrypt no primeiro login ou reset forçado de senha.
3. PHP continua aceitando sessão apenas para BCs não migrados do mesmo tenant.

### Frontend

| App | Estratégia |
| --- | ---------- |
| Painel tenant | Novo SPA (Blazor/React/Angular) consumindo API C#; telas não migradas em iframe PHP temporário |
| Painel SAS | Idem |
| PDV | Prioridade máxima de UX; reimplementar último ou usar API C# com UI mínima |

---

## Anti-Corruption Layer (resumo)

ACL detalhada em [`acl-design.md`](./acl-design.md).

| BC | Necessidade ACL | Motivo |
| -- | --------------- | ------ |
| Identity | **Alta** | MD5, `nivel` string, permissões N:N |
| Platform | **Alta** | `empresas` + `config` empresa 0 |
| Catalog | **Alta** | grades 2D, `detalhes_grade` polimórfico |
| Sales | **Crítica** | carrinho `venda=0`, venda = `receber` |
| Finance | **Alta** | `receber`/`pagar` polimórficos |
| CRM | Média | mapeamento direto |
| Settings | Média | `config` por tenant |
| Reporting | Média | SQL legado → queries CQRS |
| Notifications | Baixa | adapter HTTP externo |

**Componentes padrão por BC** (C#):

```
RetailOps.<BC>.Infrastructure.Legacy/
  Legacy<Entity>Adapter.cs      // IDataReader / EF raw
  Legacy<Entity>Mapper.cs         // DTO legado → domínio
  ILegacy<BC>Port.cs              // interface application
  Legacy<BC>DbContext.cs          // mapeamento tabelas sas (temporário)
```

---

## Critérios de Conclusão por BC

### Checklist geral (cada fase)

- [ ] Feature parity documentada vs `requirements.md`
- [ ] Testes unitários domínio ≥ 95% (`test-unit-cs`)
- [ ] Testes E2E API críticos (`test-e2e-cs`)
- [ ] Parallel run sem divergência > 0,1% (Core)
- [ ] Tráfego 100% roteado para C# (flag ON todos pilotos)
- [ ] ACL removida — EF usa schema normalizado
- [ ] PHP desabilitado para rotas do BC

### Critérios específicos — Sales (PDV)

- [ ] Abrir/fechar caixa com quebra calculada igual ao PHP
- [ ] Venda com grade, desconto %, fiado, comissão
- [ ] Cancelamento devolve estoque + grade
- [ ] Sangria refletida no fechamento
- [ ] Recibo PDF gerado

### Critérios de desligamento final (Fase 11)

- [ ] Zero tenants com flag PHP
- [ ] Backup `sas.sql` + filesystem uploads
- [ ] Monitoramento 30 dias sem rollback
- [ ] Remover `exemplos/php/vendas` do deploy (manter só arquivo histórico no repo)

---

## Riscos e mitigações

| Risco | Probabilidade | Mitigação |
| ----- | ------------- | --------- |
| Divergência estoque PDV | Alta | Parallel run + transações EF + lock otimista |
| Senha MD5 incompatível Identity | Média | Login híbrido + migração lazy bcrypt |
| `receber` polimórfica | Alta | ACL + aggregates tipados; não unificar prematuramente |
| PDF diferente dompdf | Média | Comparar amostras; QuestPDF templates |
| WhatsApp API indisponível | Baixa | Circuit breaker; fila outbox |
| Scope creep e-commerce B2C | Média | Fora de escopo explícito (ver strategic model) |

---

## Referências

| Documento | Caminho |
| --------- | ------- |
| Requisitos | `docs/discovery/ecommerce-legado-php/requirements.md` |
| Modelo estratégico | `docs/modeling/loja-php/ddd-strategic-model.md` |
| Modelo tático | `docs/modeling/loja-php/ddd-tactical-model.md` |
| ACL detalhada | `docs/migration/loja-php/acl-design.md` |
