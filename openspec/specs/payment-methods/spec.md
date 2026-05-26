# payment-methods Specification

## Purpose

Manage tenant-scoped payment methods with surcharge percent for installment calculations (RN-047), exposed to Sales BC via stable PaymentMethodId references.

## Requirements

### Requirement: Payment method CRUD

Tenant administrators SHALL create, list, update, and delete payment methods for their tenant.

#### Scenario: Create payment method

- **WHEN** administrator creates payment method with name and surcharge percent
- **THEN** method is persisted and returned with generated id

#### Scenario: Delete payment method

- **WHEN** administrator deletes unused payment method
- **THEN** method is removed for tenant

#### Scenario: Unique name per tenant

- **WHEN** administrator creates payment method with duplicate name in same tenant
- **THEN** response status is 409 or validation error

### Requirement: Surcharge calculation

The system SHALL calculate payment surcharge on base amount using stored surcharge percent per RN-047.

#### Scenario: Apply surcharge

- **WHEN** Sales BC requests surcharge calculation for amount 100.00 and method with 3% surcharge
- **THEN** calculated total is 103.00

#### Scenario: Zero surcharge

- **WHEN** payment method has 0% surcharge
- **THEN** calculated total equals base amount
