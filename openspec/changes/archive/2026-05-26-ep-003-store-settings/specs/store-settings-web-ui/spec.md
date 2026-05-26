## ADDED Requirements

### Requirement: Store configuration form

The Vue app SHALL provide a settings page for tenant store configuration with all RF-027 fields.

#### Scenario: Load and edit store config

- **WHEN** tenant admin opens store settings page
- **THEN** form loads current config and saves via PUT on submit

### Requirement: Payment methods management UI

The Vue app SHALL list payment methods in DataTable with create/edit dialog forms.

#### Scenario: Add payment method from UI

- **WHEN** admin adds payment method with name and surcharge in dialog
- **THEN** table refreshes with new row after successful API call

### Requirement: Cash registers management UI

The Vue app SHALL list cash register terminals with create/edit forms including operator selection.

#### Scenario: Create terminal from UI

- **WHEN** admin submits new terminal form with name and optional operator
- **THEN** terminal appears in list after successful API call

### Requirement: Permission-guarded routes

Store settings routes SHALL require appropriate permission keys from EP-001 RBAC.

#### Scenario: Unauthorized user blocked

- **WHEN** user without settings permission navigates to settings route
- **THEN** router redirects to unauthorized view
