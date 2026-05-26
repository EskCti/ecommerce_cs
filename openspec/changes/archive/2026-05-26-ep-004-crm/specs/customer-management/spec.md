## ADDED Requirements

### Requirement: Customer CRUD

Tenant users SHALL create, read, update, and deactivate customers scoped to their tenant.

#### Scenario: Create customer

- **WHEN** operator submits valid customer with name, CPF, and contact info
- **THEN** customer is persisted and returned with generated CustomerId

#### Scenario: Update customer contact

- **WHEN** operator updates phone and email for existing customer
- **THEN** changes are persisted for that tenant only

#### Scenario: Deactivate customer

- **WHEN** operator deactivates customer
- **THEN** customer is marked inactive and excluded from default list queries

### Requirement: Find or create by CPF

The system SHALL provide an operation that returns existing customer by CPF or creates a new one atomically within tenant scope.

#### Scenario: Existing CPF found

- **WHEN** PDV calls find-or-create with CPF already registered for tenant
- **THEN** existing customer is returned without duplicate row

#### Scenario: New CPF creates customer

- **WHEN** find-or-create is called with new valid CPF and minimum name
- **THEN** new customer is created and returned with CustomerRegistered event

#### Scenario: Invalid CPF rejected

- **WHEN** find-or-create receives invalid CPF format
- **THEN** response is validation error without creating customer

### Requirement: Customer attachments metadata

The system SHALL support listing and adding attachment metadata (tipo Cliente) linked to customer aggregate.

#### Scenario: Add attachment metadata

- **WHEN** operator adds attachment record with name and file path for customer
- **THEN** attachment is stored as child of Customer aggregate

### Requirement: Paginated customer list

The system SHALL list customers with pagination and filters by name or CPF.

#### Scenario: Search by CPF partial

- **WHEN** operator searches customers with CPF filter
- **THEN** matching customers for tenant are returned paginated
