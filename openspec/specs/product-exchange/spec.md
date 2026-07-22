## ADDED Requirements

### Requirement: Register product exchange

Operators SHALL register 1:1 product exchange with stock adjustment per RF-060 and RN-050.

#### Scenario: Successful exchange

- **WHEN** operator submits valid customer, product in, product out, and sufficient stock on product out
- **THEN** exchange is persisted, product in stock increases by 1, product out stock decreases by 1

#### Scenario: Fixed quantity one each side

- **WHEN** operator attempts exchange with quantity other than 1
- **THEN** validation fails per RN-050 invariant

#### Scenario: Insufficient outbound stock

- **WHEN** product out has zero available stock
- **THEN** registration fails with validation error

### Requirement: Graded product exchange

When products have grades, operators SHALL specify grade selection for in and out products per RF-060.

#### Scenario: Exchange with grades

- **WHEN** operator selects grade options for both products
- **THEN** stock adjustment applies to selected grade options and Troca Entrada/Saída grade details are recorded

### Requirement: Auto-create customer by CPF

When CPF is provided and customer does not exist, the system SHALL create customer automatically per RN-051.

#### Scenario: New customer on exchange

- **WHEN** operator provides CPF and name without existing customer
- **THEN** CRM FindOrCreateByCpf is invoked and exchange links to resulting CustomerId

### Requirement: Product exchanged event

Successful exchange SHALL publish ProductExchanged domain event.

#### Scenario: Event published

- **WHEN** exchange registers successfully
- **THEN** ProductExchanged event is published with exchange id and product references
