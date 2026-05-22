## ADDED Requirements

### Requirement: Vue admin application exists

The repository SHALL contain `apps/web-vue` with Vue 3, Vue Router, Pinia, Tailwind CSS v4, and PrimeVue 4 configured.

#### Scenario: Dev server starts

- **WHEN** developer runs the Vue dev server from `apps/web-vue`
- **THEN** the application loads in the browser without build errors

### Requirement: Admin shell layout

The web app SHALL render a collapsible sidebar, topbar, and footer on authenticated layout routes.

#### Scenario: Dashboard layout visible

- **WHEN** user navigates to the main dashboard route
- **THEN** sidebar, topbar, and footer are visible and responsive

### Requirement: API proxy to C# backend

The Vite dev server SHALL proxy `/api` requests to `http://localhost:5000`.

#### Scenario: Proxied API call

- **WHEN** the Vue app requests `/api/health` during development
- **THEN** the request is forwarded to the ASP.NET Core API on port 5000
