## ADDED Requirements

### Requirement: Base entity type

The shared kernel SHALL provide an abstract `Entity` base class with identity support for domain aggregates.

#### Scenario: Entity creation in domain layer

- **WHEN** a future bounded context defines an aggregate inheriting from `Entity`
- **THEN** the aggregate compiles without referencing infrastructure packages

### Requirement: Result type for domain operations

The shared kernel SHALL provide `Result<T>` (and non-generic `Result`) for expressing success and failure without exceptions in domain logic.

#### Scenario: Failed validation returns Result

- **WHEN** a value object factory receives invalid input
- **THEN** it returns a failed `Result` with an error message instead of throwing

### Requirement: Application contracts

The shared kernel SHALL define `IUseCase` and `IRepository` interfaces used by application and infrastructure layers.

#### Scenario: Use case implements contract

- **WHEN** an application service implements `IUseCase<TRequest, TResponse>`
- **THEN** the API layer can invoke it through the interface without concrete coupling

### Requirement: Transaction boundary

The shared kernel SHALL provide `ITransactionManager` (or equivalent) for coordinating unit-of-work across repositories.

#### Scenario: Transaction scope

- **WHEN** a use case runs inside `RunInTransaction`
- **THEN** repository operations within the delegate share a single transaction boundary
