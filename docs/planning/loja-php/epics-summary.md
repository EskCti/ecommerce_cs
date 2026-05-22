# Resumo de Épicos — RetailOps (Loja PHP)

**Stack**: C# · Vue 3 + PrimeVue · Kotlin Android  
**Data**: 2026-05-22

| ID | Épico | BC | Tipo | Tamanho | Stories | Tasks (est.) | Dep. | Release |
| -- | ----- | -- | ---- | ------- | ------- | ------------ | ---- | ------- |
| EP-000 | Bootstrap e Infra | TECH | Enabler | M | 2 | ~25 | — | R0 |
| EP-001 | Identity & Access | Auth | Generic | M | 3 | ~45 | EP-000 | R1 |
| EP-002 | Platform (SaaS) | Platform | Core | M | 3 | ~40 | EP-001 | R1 |
| EP-003 | Store Settings | Settings | Supporting | P | 2 | ~30 | EP-001 | R1 |
| EP-004 | CRM | CRM | Supporting | P | 2 | ~36 | EP-001 | R2 |
| EP-005 | Catalog & Inventory | Catalog | Core | G | 4 | ~70 | EP-003 | R1 |
| EP-006 | Sales (PDV) | Sales | Core | GG | 5 | ~110 | EP-003, EP-005 | R1 |
| EP-007 | Finance | Finance | Supporting | G | 3 | ~66 | EP-006 | R2 |
| EP-008 | Returns | Returns | Supporting | P | 1 | ~18 | EP-004, EP-005 | R2 |
| EP-009 | Reporting | Reporting | Generic | M | 2 | ~32 | EP-007 | R3 |
| EP-010 | Notifications | Notifications | Generic | P | 1 | ~14 | EP-007 | R3 |
| EP-011 | Cutover PHP | TECH | Enabler | M | 1 | ~18 | EP-001–010 | R4 |

**Totais**: 38 stories · ~504 tasks · ~18–22 sprints (2–3 devs)

## MVP (Release 1)

EP-000 → EP-001 → EP-002 → EP-003 → EP-005 → EP-006

**Entrega MVP**: trial + login + produtos + PDV (caixa, venda, fechamento) com parallel run vs PHP.

## Mapa épicos ↔ subdomínios

| Subdomínio (estratégico) | Épico |
| ------------------------ | ----- |
| SD-08 Identidade e Acesso | EP-001 |
| SD-03 Gestão SaaS | EP-002 |
| SD-07 Configuração da Loja | EP-003 |
| SD-06 Cadastro de Partes | EP-004 |
| SD-02 Catálogo e Estoque | EP-005 |
| SD-01 Operação de PDV | EP-006 |
| SD-04 Financeiro da Loja | EP-007 |
| SD-05 Trocas | EP-008 |
| SD-09 Relatórios | EP-009 |
| SD-10 Notificações | EP-010 |

## Skills C# principais por release

| Release | Skills |
| ------- | ------ |
| R0 | Config Project (C#), Config EF Core (C#), Config Docker (C#), Config CI/CD (C#), Config Shared Core (C#), Config Shared Web (Vue), Config Project (Android) |
| R1 | Config Auth Core (C#), Config Auth Backend Basic (C#), Config New Module (C#), Core * (C#), Backend Data (C#), Backend Controller (C#), Frontend * (Vue), Mobile * (Android) |
| R2–R3 | Idem + Core Query CQRS (C#), E2E Tests (C#) |
| R4 | Migração manual + test regressão |
