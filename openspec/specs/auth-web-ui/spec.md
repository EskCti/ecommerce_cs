# auth-web-ui Specification

## Purpose

Vue admin UI for login, permission-based route guards, and user permission management (EP-001 Identity).
## Requirements
### Requirement: Public login page

The Vue app SHALL provide a public login page at `/login` with email/CPF and password fields using PrimeVue inputs.

#### Scenario: Successful login redirect

- **WHEN** user submits valid credentials on login page
- **THEN** JWT is stored and user is redirected to tenant dashboard or SAS panel based on user level

### Requirement: Route guard by permission key

The Vue router SHALL block navigation to routes when user lacks required `PermissionKey`.

#### Scenario: Blocked route access

- **WHEN** user without `financeiro.receber` permission navigates to receivables route
- **THEN** router redirects to unauthorized page or dashboard

### Requirement: User and permission management UI

Administrators SHALL manage user permissions through Vue pages with PrimeVue DataTable and forms.

#### Scenario: Permission assignment in UI

- **WHEN** administrator selects permissions and saves on user edit form
- **THEN** UI calls API and reflects updated grants without full page reload

