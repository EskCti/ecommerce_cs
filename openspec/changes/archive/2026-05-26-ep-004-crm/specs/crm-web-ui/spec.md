## ADDED Requirements

### Requirement: Customers management pages

The Vue app SHALL provide customer list and create/edit forms using PrimeVue DataTable and vee-validate.

#### Scenario: Customer list loads

- **WHEN** operator navigates to /crm/customers
- **THEN** DataTable displays paginated customers with search by name/CPF

#### Scenario: Customer form saves

- **WHEN** operator submits valid customer form
- **THEN** API persists customer and UI returns to list or shows success

### Requirement: Suppliers management pages

The Vue app SHALL provide supplier list and forms with person type selector (F/J).

#### Scenario: Supplier form person type switch

- **WHEN** operator selects Legal entity person type
- **THEN** form shows CNPJ field and hides CPF-specific validation

### Requirement: Permission-guarded CRM routes

CRM routes SHALL require RBAC permission keys aligned with legacy clientes/fornecedores menus.

#### Scenario: Missing permission blocks CRM page

- **WHEN** user without clientes.listar permission opens customers page
- **THEN** router redirects to unauthorized view
