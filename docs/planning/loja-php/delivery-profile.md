# Perfil de entrega — RetailOps (Loja PHP)

**Data**: 2026-05-22  
**Legado analisado**: `docs/discovery/ecommerce-legado-php/` · `exemplos/php/vendas/`

## Stack alvo (fixa para o backlog)

| Camada | Tecnologia | Pasta alvo |
|--------|------------|------------|
| Backend | ASP.NET Core 8 + EF Core | `apps/backend` |
| Web admin | Vue 3 + PrimeVue 4 | `apps/web-vue` |
| Mobile | Kotlin + Compose + Hilt + Retrofit | `apps/mobile-android` |
| Banco | PostgreSQL (`sas_vendas`) + ACL legado MariaDB `sas` | — |

**Sufixo de skills**: `-cs` · agents `Frontend * (Vue)` e `Mobile * (Android)` sem sufixo.

## Superfícies por Bounded Context (Release 1 — MVP)

| BC | API REST | Web admin | Mobile | Observação |
|----|----------|-----------|--------|------------|
| EP-000 Bootstrap | Sim (health) | Sim (shell) | Sim (app vazio) | Enabler |
| EP-001 Identity & Access | Sim | Sim (login, usuários/permissões) | Sim (perfil `/me`) | Desbloqueia demais BCs |
| EP-002 Platform (SaaS) | Sim | Sim (empresas SAS) | Não | Trial self-service |
| EP-003 Store Settings | Sim | Sim | Não | Pré-requisito PDV |
| EP-005 Catalog | Sim | Sim | Não (consulta via API no PDV) | — |
| EP-006 Sales (PDV) | Sim | Parcial (relatórios/config) | Sim (venda, caixa) | Core |

BCs Release 2+ seguem a mesma tabela ao entrar no backlog; atualizar este arquivo quando priorizar novo release.

## Inventário de UI (legado)

- **Web**: login (`sistema/login`), painel tenant, painel SAS (`empresa=0`), menus por `acessos.chave`, cadastros, PDV web, relatórios.
- **Mobile**: operação de venda em campo (paridade futura com API RetailOps); perfil/consulta usuário no MVP Android.
- **Somente API / batch**: ETL, integrações fiscais legado — fora do MVP web/mobile.

## Regras aplicadas ao `backlog.md`

- Cada US com coluna **Web = Sim** inclui bloco Vue completo (entity → repository → page/form).
- Cada US com coluna **Mobile = Sim** inclui bloco Android completo (entity → screen).
- Não usar linha única “Template full-stack”; ver seção template no `backlog.md`.
