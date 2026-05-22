## Context

**RetailOps** — BC **CRM** (Supporting, Cadastro de Partes). Release 2 no backlog; implementado após MVP auth/settings/catalog/sales ou em paralelo quando pilot tenant exigir clientes.

Legado:
- `clientes` — RF-020 (nome, CPF, contatos, endereço; histórico vendas/contas/arquivos)
- `fornecedores` — RF-021 (pessoa F/J)
- `arquivos` tipo=`Cliente` — anexos do cliente

Referências: `ddd-tactical-model.md` (BC CRM), `acl-design.md` (BC CRM).

**Nota:** RF-022 funcionários e RF-023 usuários pertencem a Identity EP-001, não CRM.

## Goals / Non-Goals

**Goals:**

- CRUD Customer e Supplier tenant-scoped
- `FindOrCreateByCpfUseCase` idempotente para PDV
- `CustomerRegistrationPolicy` (CPF válido, unicidade por tenant)
- ACL mappers legado
- UI Vue completa; Android clientes (US-040)
- Queries de listagem paginada com filtros nome/CPF

**Non-Goals:**

- Histórico vendas/contas inline (EP-006 Sales / EP-007 Finance — links/read models futuros)
- Upload avançado de anexos (MVP: metadados + path; upload file EP posterior)
- Fornecedor mobile (US-041 web only)
- Integração contábil externa

## Decisions

### 1. Módulo Crm

**Decisão:** `Config New Module (C#)` → `RetailOps.Crm.*`; todos repositórios filtram `TenantId`.

### 2. Customer aggregate

**Decisão:** `Customer` AR: `PersonName`, `Cpf`, contatos, `Address`; child `Attachment` (nome, path, tipo Cliente); métodos `UpdateContactInfo()`, `AddAttachment()`, `Deactivate()`.

**FindOrCreateByCpf:** se CPF existe no tenant → retorna existente + evento `CustomerFoundByCpf`; senão cria + `CustomerRegistered`.

### 3. Supplier aggregate

**Decisão:** `Supplier` AR com `PersonType` (Individual|Company), `TaxDocument` (CPF ou CNPJ conforme tipo), contatos.

### 4. API design

**Decisão:**
- `GET/POST/PUT/DELETE /api/crm/customers`
- `POST /api/crm/customers/find-or-create-by-cpf`
- `GET/POST/PUT/DELETE /api/crm/suppliers`
- `GET /api/crm/customers/{id}/attachments` (list/add MVP)

Autorização: permissões legado mapeadas (`clientes.*`, `fornecedores.*`).

### 5. Cross-BC integration

**Decisão:** Sales BC chama `FindOrCreateByCpfUseCase` via application port ou HTTP interno; domínio Sales guarda apenas `CustomerId` VO.

### 6. Frontend Vue

**Decisão:** Rotas `/crm/customers`, `/crm/suppliers`; template full-stack do backlog (Entity → UseCase → Repository → Page → Form).

### 7. Mobile Android (US-040)

**Decisão:** Feature `customers`: list + form Compose; Retrofit autenticado; sem supplier mobile.

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| CPF duplicado cross-tenant | Unique index (tenant_id, cpf) |
| FindOrCreate race condition | Transaction + unique constraint |
| Histórico RF-020 scope creep | Links para Sales/Finance em UI placeholder; dados em EP-006/007 |
| Attachment storage | MVP path string; S3 later |

## Migration Plan

1. Domain → infra → API → Vue → Android → tests
2. Pilot flag CRM BC
3. PDV EP-006 consome find-or-create

**Rollback:** pilot off; PHP `clientes/` / `fornecedores/` ativos.

## Open Questions

- CPF obrigatório em todo cliente? **Sim, alinhado legado PDV fiado**
- Supplier CNPJ validation strict? **Usar TaxDocument VO shared**
