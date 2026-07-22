## ADDED Requirements

### Requirement: Once per day execution

The system SHALL send at most one digest per tenant per calendar day per RN-070.

#### Scenario: Skip already sent today

- **WHEN** daily job runs and DailyDigestLog exists for tenant and today
- **THEN** handler skips send and returns without calling WhatsApp gateway

#### Scenario: Record after successful send

- **WHEN** digest sends successfully
- **THEN** DailyDigestLog entry is persisted with tenant id and digest date

### Requirement: Scheduled background job

Digest job SHALL run automatically once per day via hosted background service.

#### Scenario: Scheduled execution

- **WHEN** configured daily schedule time is reached in America/Sao_Paulo timezone
- **THEN** SendDailyTenantDigestHandler processes all eligible tenants

### Requirement: Manual admin trigger

Admin users SHALL trigger digest manually for testing via protected endpoint.

#### Scenario: Admin manual trigger

- **WHEN** admin calls POST /api/notifications/digest/trigger for tenant
- **THEN** digest runs ignoring schedule but still respects RN-070 unless force flag provided
