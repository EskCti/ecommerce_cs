## Why

O BC **CRM** (Cadastro de Partes, Supporting) centraliza clientes e fornecedores usados por vendas fiado (EP-006), financeiro (EP-007) e catálogo (fornecedor em produtos). Sem CRUD de `clientes`/`fornecedores` e o caso de uso **`FindOrCreateByCpf`** (PDV), operadores não conseguem vincular partes às transações — paridade com RF-020 e RF-021 do legado.

## What Changes

- Novo módulo `RetailOps.Crm` (Core, Application, Infrastructure)
- Agregados `Customer` e `Supplier` com VOs compartilhados (`PersonName`, `Cpf`, `Email`, `Phone`, `Address`, `PersonType`)
- Serviço `CustomerRegistrationPolicy`; eventos `CustomerRegistered`, `CustomerFoundByCpf`
- Caso de uso **`FindOrCreateByCpfUseCase`** para PDV e cadastro rápido
- ACL legado: `clientes`, `fornecedores`, `arquivos` (tipo Cliente) via `ICrmLegacyPort`
- API tenant-scoped: CRUD customers/suppliers + endpoint find-or-create by CPF
- Frontend Vue: listagem/form clientes e fornecedores (template full-stack)
- Mobile Android: listagem/cadastro cliente (US-040 template)
- Testes unitários ≥95% e E2E CRUD + find-or-create

## Capabilities

### New Capabilities

- `customer-management`: CRUD clientes, FindOrCreateByCpf, anexos básicos (tipo Cliente)
- `supplier-management`: CRUD fornecedores pessoa Física/Jurídica
- `crm-web-ui`: Páginas Vue DataTable + forms clientes e fornecedores
- `crm-mobile-customers`: Telas Android listagem/cadastro cliente

### Modified Capabilities

_(nenhuma — BC novo; Sales/Finance consumirão CustomerId via referência em EP-006/007)_

## Impact

- **Backend**: módulo Crm + ACL parallel run
- **Frontend/Mobile**: features CRM no admin tenant
- **Sales BC (EP-006)**: depende de `FindOrCreateByCpf` e CustomerId
- **Catalog BC (EP-005)**: referência opcional SupplierId em produtos
- **Dependências**: EP-000 bootstrap, EP-001 auth
- **Release**: 2 (pós-MVP core PDV, conforme backlog)
