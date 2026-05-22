# Inventário de entrega — E-commerce legado PHP (SAS Vendas)

**Fonte**: `docs/discovery/ecommerce-legado-php/` · `exemplos/php/vendas/`  
**Data**: 2026-05-22

## Resumo

| Superfície | Presente no legado? | Observação |
|------------|-------------------|------------|
| Painel web admin | Sim | PHP `sistema/`, `sas/`, menus por `acessos` |
| App mobile / PDV campo | Parcial | Legado web-first; app Android é alvo RetailOps |
| API/integrações only | Sim | Fiscal, batch, coexistência strangler |

## Telas web por área de negócio

| Área (≈ BC) | Telas / rotas principais | CRUD? | Relatórios? |
|-------------|--------------------------|-------|-------------|
| Identity & Access | login, usuários, cargos, permissões | Sim | — |
| Platform (SAS) | empresas, cobrança tenant, config global | Sim | Sim |
| Store Settings | config loja, formas pgto, caixas | Sim | — |
| CRM | clientes, fornecedores | Sim | — |
| Catalog | produtos, categorias, marcas, estoque | Sim | Sim |
| Sales (PDV) | abertura/fechamento caixa, venda, orçamento | Sim | Sim |
| Finance | receber, pagar, caixa | Sim | Sim |
| Reporting | `rel/`, `rel_sistema/` | — | Sim |

## Mobile (alvo RetailOps)

| Fluxo | Telas legado | Paridade MVP RetailOps |
|-------|--------------|------------------------|
| Auth / perfil | — (web) | Sim (`/api/auth/me`) |
| PDV venda | web responsivo / futuro app | Release 1+ (EP-006) |

## Próximo passo

→ `docs/planning/loja-php/delivery-profile.md`  
→ `req-agile-planning` gera backlog com tasks Vue + Android por US quando coluna Web/Mobile = Sim
