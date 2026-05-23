# tenant-billing-suspension Specification

## Purpose

Tenant billing (receivables), overdue suspension, user deactivation on suspension, and configurable block messages.

## Requirements

### Requirement: Issue tenant invoice

The system SHALL create a platform receivable (legacy `receber` tipo Empresa) for tenant monthly fee.

#### Scenario: Invoice issued for tenant

- **WHEN** SAS operator or scheduled job triggers invoice for active company
- **THEN** receivable record is created linked to company with correct amount and due date

### Requirement: Suspend overdue tenant

The system SHALL suspend tenants overdue beyond configured block days (`dias_bloqueio`) with configurable block message.

#### Scenario: Overdue tenant suspended

- **WHEN** company has unpaid invoice past dias_bloqueio threshold
- **THEN** company is marked suspended and TenantSuspended domain event is published

### Requirement: User deactivation on suspension

When tenant is suspended, all users of that tenant SHALL be deactivated via Identity integration.

#### Scenario: Users deactivated after suspension

- **WHEN** TenantSuspended event is handled
- **THEN** all users for that tenant id are marked inactive

### Requirement: Block message from platform config

Suspended tenants SHALL receive block message from platform configuration `msg_bloqueio` when attempting access.

#### Scenario: Block message returned on login

- **WHEN** user of suspended tenant attempts login
- **THEN** response includes configured block message (via auth integration)
