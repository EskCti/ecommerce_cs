## ADDED Requirements

### Requirement: EF Core DbContext registered

The infrastructure layer SHALL register a primary `ApplicationDbContext` with EF Core and connection string from configuration.

#### Scenario: DbContext resolves at startup

- **WHEN** the API starts with valid `ConnectionStrings__DefaultConnection`
- **THEN** `ApplicationDbContext` is available via dependency injection

### Requirement: Initial migration applies

The project SHALL ship an initial EF Core migration that creates the bootstrap schema (empty or minimal tenant tables).

#### Scenario: Database update

- **WHEN** developer runs `dotnet ef database update`
- **THEN** the database schema is created without errors

### Requirement: Dev database via Docker Compose

The repository SHALL include `docker-compose.yml` with a Postgres service for local development.

#### Scenario: Compose up

- **WHEN** developer runs `docker compose up -d`
- **THEN** Postgres is reachable on the configured port with credentials from `.env.example`
