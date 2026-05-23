# rbac-permissions Specification

## Purpose

Manage granular permission grants per user within a tenant, aligned with the legacy `acessos` catalog and privileged level bypass rules.

## Requirements

### Requirement: Permission catalog seeded

The system SHALL seed the permission catalog with the 35 legacy `acessos` entries (key, name, group).

#### Scenario: Catalog available after migration

- **WHEN** database is initialized with auth migrations
- **THEN** permission catalog contains all 35 legacy access keys

### Requirement: Assign permissions to user

Administrators SHALL assign and revoke permission grants for users within their tenant via API.

#### Scenario: Assign permission grant

- **WHEN** administrator assigns permission key `produtos.listar` to a user
- **THEN** grant is persisted and appears in user's effective permissions

#### Scenario: Revoke permission grant

- **WHEN** administrator removes a permission grant from a user
- **THEN** grant is deleted and user no longer has that permission key

### Requirement: Privileged level bypass

Users with level SAS or Administrador SHALL bypass granular permission grant requirements for authorization.

#### Scenario: SAS user access without grants

- **WHEN** SAS-level user has zero permission grants
- **THEN** authorization policy still allows access to tenant-scoped operations per SAS rules

### Requirement: List user permissions

The system SHALL expose query endpoints to list users and their permission grants for admin UI.

#### Scenario: List permissions for user

- **WHEN** administrator requests permissions for a user in their tenant
- **THEN** response lists all assigned permission keys with catalog metadata
