## ADDED Requirements

### Requirement: Exchange registration form

The Vue app SHALL provide exchange registration form with customer CPF, products in/out, and optional grade selectors per RF-060.

#### Scenario: Submit exchange from form

- **WHEN** operator completes valid exchange form
- **THEN** UI calls POST /api/returns/exchanges and shows success

### Requirement: Exchange list page

The Vue app SHALL list exchanges with delete action per RF-061.

#### Scenario: Delete from list

- **WHEN** authorized user confirms delete on exchange row
- **THEN** UI calls DELETE API and refreshes list

### Requirement: Permission-guarded routes

Returns routes SHALL require RBAC permissions aligned with legacy trocas menu.

#### Scenario: Unauthorized access blocked

- **WHEN** user without trocas permission opens returns page
- **THEN** router redirects to unauthorized view
