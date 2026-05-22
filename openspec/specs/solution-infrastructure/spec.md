## Requirements

### Requirement: Solution builds successfully

The repository SHALL contain a `RetailOps.sln` solution with Clean Architecture projects that compile without errors.

#### Scenario: Build from CLI

- **WHEN** developer runs `dotnet build` at repository root
- **THEN** all projects in the solution compile successfully

### Requirement: Health endpoint responds

The API SHALL expose `GET /health` returning HTTP 200 with a JSON payload indicating service status.

#### Scenario: Health check success

- **WHEN** client sends `GET /health` to the running API
- **THEN** response status is 200 and body indicates the service is healthy

### Requirement: Test projects exist

The solution SHALL include `RetailOps.UnitTests` and `RetailOps.IntegrationTests` projects referenced in the solution file.

#### Scenario: Test discovery

- **WHEN** developer runs `dotnet test`
- **THEN** test runner discovers and executes tests in both test projects
