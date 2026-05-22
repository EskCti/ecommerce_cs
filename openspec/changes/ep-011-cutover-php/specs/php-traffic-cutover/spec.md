## ADDED Requirements

### Requirement: Disable PHP routing

Production deployment SHALL route all tenant and platform traffic to C# API and Vue SPA with zero requests forwarded to PHP per US-110.

#### Scenario: PHP routes removed

- **WHEN** cutover completes in production
- **THEN** YARP or Nginx configuration contains no upstream routes to PHP application

#### Scenario: SPA serves tenant UI

- **WHEN** user navigates to former PHP tenant paths
- **THEN** Vue SPA handles routing without PHP iframe fallback

### Requirement: Legacy PHP disabled flag

Configuration SHALL set Migration LegacyPhpEnabled to false for all environments after cutover.

#### Scenario: Global PHP disabled

- **WHEN** production cutover flag is applied
- **THEN** no tenant is eligible for legacy PHP routing

### Requirement: PHP container removed from production

Production docker-compose or orchestration SHALL not include PHP-FPM or Apache PHP service after cutover.

#### Scenario: Production stack C# only

- **WHEN** production deployment is inspected post-cutover
- **THEN** only RetailOps API, Vue, Postgres, and supporting infra services are running
