## ADDED Requirements

### Requirement: Pre-cutover backup

Operators SHALL take full database dump of sas schema and archive filesystem uploads before ACL removal.

#### Scenario: Database backup verified

- **WHEN** decommission runbook pre-cutover step executes
- **THEN** pg_dump artifact exists and restore test succeeds in staging

#### Scenario: Uploads backup verified

- **WHEN** decommission runbook pre-cutover step executes
- **THEN** uploads directory tarball exists with checksum recorded

### Requirement: Decommission runbook

Project SHALL include documented runbook for PHP decommission with rollback procedure.

#### Scenario: Runbook covers rollback

- **WHEN** operator follows rollback section within 4 hour window
- **THEN** PHP routing and database can be restored from backup

### Requirement: Post-cutover monitoring period

Operations SHALL monitor production for 30 consecutive days after cutover without rollback per US-110 acceptance criteria.

#### Scenario: Monitoring dashboard active

- **WHEN** cutover completes
- **THEN** alerts cover API errors, sales volume deviation, and stock discrepancy for 30 days

#### Scenario: Rollback trigger documented

- **WHEN** critical business metric deviates beyond threshold for 24 hours
- **THEN** runbook rollback procedure is initiated
