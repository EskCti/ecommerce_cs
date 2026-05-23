# platform-sas-web-ui Specification

## Purpose

Vue admin UI for platform operations: trial registration, SAS company list, and company/contract management forms.

## Requirements

### Requirement: Trial registration form

The Vue app SHALL provide a public trial registration form (modal or page) collecting company and admin user data.

#### Scenario: Trial form submission success

- **WHEN** prospect completes trial form with valid data
- **THEN** UI calls trial API and shows success with redirect to login

#### Scenario: Trial form validation errors

- **WHEN** prospect submits invalid CNPJ or duplicate email
- **THEN** form displays inline validation errors from API

### Requirement: SAS companies list page

SAS operators SHALL manage companies via Vue page with PrimeVue DataTable, filters, and pagination.

#### Scenario: Companies table loads

- **WHEN** SAS user navigates to SAS companies route
- **THEN** DataTable displays paginated companies from platform API

### Requirement: Company edit and contract forms

SAS operators SHALL edit company details and manage contracts through Vue forms with vee-validate.

#### Scenario: Save company from form

- **WHEN** SAS user edits company and submits form
- **THEN** changes persist via API and UI reflects updated data
