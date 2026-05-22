# Plano de Sprint — RetailOps (Loja PHP)

**Capacidade assumida**: 2 devs · sprint 2 semanas · ~20 pontos/sprint  
**Stack**: C# + Vue PrimeVue + Android Kotlin

---

## Sprint 1 (Release 0)

**Objetivo**: Projeto compilando com CI, shell Vue, app Android base, ACL DbContext.

| Story | Pontos | Foco |
| ----- | ------ | ---- |
| US-000 Setup full-stack | 8 | EP-000 bootstrap completo |
| US-001 Multi-tenancy | 5 | TenantId middleware + flags |
| US-010 Login JWT (início) | 5 | Auth core + endpoint login |

**Total**: 18 pontos

**Entregáveis**:
- `RetailOps.sln` + `/health`
- Vue shell admin navegável
- Android app com splash + config API
- CI verde em PR

---

## Sprint 2 (Release 1 — Auth + Platform)

**Objetivo**: Login funcional com ACL legado; trial registration; SAS list companies.

| Story | Pontos | Foco |
| ----- | ------ | ---- |
| US-010 Login JWT (conclusão) | 3 | E2E auth + tela login Vue |
| US-011 RBAC | 5 | Permissões + guards Vue |
| US-012 Manager PIN | 3 | Prerequisite caixa |
| US-020 Trial | 5 | RegisterTrialCompany |
| US-021 SAS empresas (parcial) | 5 | List + Create company API/Vue |

**Total**: 21 pontos

---

## Sprint 3 (Release 1 — Settings + Catalog início)

**Objetivo**: Formas pgto, caixas, CRUD produtos.

| Story | Pontos | Foco |
| ----- | ------ | ---- |
| US-030 Config loja | 5 | StoreConfig |
| US-031 Formas pgto + caixas | 5 | PDV prerequisites |
| US-050 Produtos (parcial) | 8 | Domain + API + list Vue |
| US-021 SAS empresas (conclusão) | 3 | Contratos |

**Total**: 21 pontos

---

## Sprint 4–5 (Release 1 — Catalog + PDV)

**Objetivo**: Grades, estoque, PDV MVP com parallel run.

| Sprint | Stories principais | Pontos |
| ------ | ------------------ | ------ |
| S4 | US-050 conclusão, US-051 Grades, US-052 movimentação | ~21 |
| S5 | US-060 Abrir caixa, US-061 Carrinho, US-062 Finalizar (início) | ~21 |

---

## Sprint 6+ (Release 2 em diante)

Seguir ordem do backlog: concluir US-062/063/064 → EP-007 Finance → EP-004 CRM → EP-008 Returns → EP-009/010 → EP-011 cutover.

**Marco Release 1 (MVP)**: fim Sprint 5 — PDV finalizando venda em tenant piloto com parallel run iniciado.

**Marco Release 2**: Sprint 8–10 — financeiro + CRM operacional.

**Marco Release 4**: Sprint 18–22 — desligamento PHP (EP-011).

---

## Rastreabilidade OpenSpec sugerida

| Sprint | Mudança OpenSpec |
| ------ | ---------------- |
| S1 | `bootstrap-retailops` |
| S2 | `ep-001-auth`, `ep-002-platform` (parcial) |
| S3–S4 | `ep-003-settings`, `ep-005-catalog` |
| S5–S6 | `ep-006-sales-pdv` |
