# platform-company-management Specification

## Purpose

SAS operator company lifecycle management: CRUD, listing, and authorization scoped to platform (TenantId 0).

## Requirements

### Requirement: SAS company CRUD

SAS operators SHALL create, read, update, and deactivate tenant companies via authenticated platform API.

#### Scenario: Create company

- **WHEN** SAS operator submits valid company data to create endpoint
- **THEN** company is persisted and returned with assigned tenant id

#### Scenario: Update company

- **WHEN** SAS operator updates company billing or contact fields
- **THEN** changes are persisted via ACL or EF repository

#### Scenario: List companies paginated

- **WHEN** SAS operator requests company list with pagination and filters
- **THEN** response returns paginated companies with metadata

### Requirement: SAS-only authorization

Company management endpoints SHALL require authenticated user with SAS platform scope (TenantId 0, SAS level).

#### Scenario: Tenant user denied SAS endpoints

- **WHEN** tenant-scoped user calls SAS company management API
- **THEN** response status is 403
