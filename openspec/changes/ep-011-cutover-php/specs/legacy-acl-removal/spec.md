## ADDED Requirements

### Requirement: Remove legacy infrastructure projects

After ETL validation and traffic cutover, the solution SHALL remove Infrastructure.Legacy projects and legacy DI registrations per acl-design.md schedule.

#### Scenario: Legacy projects deleted

- **WHEN** ACL removal phase completes
- **THEN** no RetailOps.*.Infrastructure.Legacy projects remain in solution

#### Scenario: EF repositories only

- **WHEN** application handlers resolve repositories post-removal
- **THEN** only normalized EF Core implementations are registered

### Requirement: Dual-write disabled

Sales and other dual-write paths SHALL be disabled before ACL removal.

#### Scenario: No dual-write after cutover

- **WHEN** legacy ACL is removed
- **THEN** SaleParallelRunLogger and dual-write adapters are not registered

### Requirement: MD5 auth path removed

Identity SHALL remove LegacyMd5PasswordVerifier after cutover; login accepts bcrypt hashes only.

#### Scenario: MD5 login rejected

- **WHEN** user with MD5-only hash attempts login post-cutover
- **THEN** login fails with password reset guidance
