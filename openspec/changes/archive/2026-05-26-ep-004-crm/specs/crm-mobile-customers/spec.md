## ADDED Requirements

### Requirement: Customer list screen

The Android app SHALL display paginated customer list consumindo CRM API.

#### Scenario: List loads with auth

- **WHEN** operator opens customer list with valid JWT
- **THEN** LazyColumn shows customers from GET /api/crm/customers

### Requirement: Customer quick registration

The Android app SHALL support creating or finding customer by CPF from mobile form.

#### Scenario: Find or create from mobile

- **WHEN** operator submits CPF and name on mobile form
- **THEN** app calls find-or-create endpoint and displays resulting customer

### Requirement: Pull to refresh

Customer list SHALL support refresh to reload data from API.

#### Scenario: Refresh list

- **WHEN** operator pulls to refresh on customer list
- **THEN** ViewModel reloads customers from repository
