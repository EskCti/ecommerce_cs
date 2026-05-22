## ADDED Requirements

### Requirement: Legacy sale adapter orchestration

Infrastructure SHALL orchestrate legacy-compatible transactions for open session, cart items, finalize, and close via ISalesLegacyPort.

#### Scenario: Finalize dual-write transaction

- **WHEN** FinalizeSaleUseCase completes successfully
- **THEN** LegacySaleAdapter writes receber, updates itens_venda, inserts commissions, and adjusts stock in single transaction boundary

### Requirement: Cart pending items mapping

ACL SHALL map cart lines to legacy itens_venda with venda=0 until finalize sets venda reference.

#### Scenario: Pending cart item in legacy

- **WHEN** item is added to cart during parallel run
- **THEN** legacy row exists with venda=0 semantics equivalent to PHP

### Requirement: Parallel run logger

The system SHALL provide SaleParallelRunLogger comparing C# sale totals to legacy shadow reads.

#### Scenario: Log comparison on finalize

- **WHEN** sale finalizes in pilot tenant
- **THEN** logger persists both totals, difference percent, and session id

### Requirement: Domain isolation from legacy names

Sales domain layer SHALL NOT reference legacy table or column names.

#### Scenario: Domain project references

- **WHEN** Sales.Core project dependencies are inspected
- **THEN** no reference to Legacy infrastructure mappers exists
