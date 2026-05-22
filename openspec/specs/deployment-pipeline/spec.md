## Requirements

### Requirement: Production Dockerfile

The repository SHALL include a multi-stage Dockerfile that builds and publishes the API for production deployment.

#### Scenario: Docker build

- **WHEN** developer runs `docker build` using the API Dockerfile
- **THEN** the image builds successfully and exposes the API port

### Requirement: Production compose file

The repository SHALL include `docker-compose.prod.yml` (or equivalent) orchestrating API, web, and database services for production-like runs.

#### Scenario: Production stack starts

- **WHEN** operator runs production compose with required env vars
- **THEN** API and dependent services start without manual assembly

### Requirement: CI runs tests on pull request

GitHub Actions SHALL run `dotnet test` on every pull request to protected branches.

#### Scenario: PR triggers CI

- **WHEN** a pull request is opened or updated
- **THEN** the CI workflow executes restore, build, and test jobs

### Requirement: CD builds Docker image on main

GitHub Actions SHALL build and push (or publish artifact for) the API Docker image when changes merge to `main`.

#### Scenario: Main branch merge

- **WHEN** commits land on `main`
- **THEN** the CD workflow completes a successful Docker build
