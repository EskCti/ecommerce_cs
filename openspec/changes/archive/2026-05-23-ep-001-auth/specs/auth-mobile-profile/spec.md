## ADDED Requirements

### Requirement: Profile screen displays authenticated user

The Android app SHALL provide a profile screen showing data from `GET /api/auth/me`.

#### Scenario: Profile loads after login

- **WHEN** user opens profile screen with valid stored token
- **THEN** screen displays name, email, user level, and tenant id

### Requirement: Authenticated API calls

The Android Retrofit client SHALL attach Bearer token from secure storage on authenticated requests.

#### Scenario: Me request with token

- **WHEN** profile ViewModel loads user data
- **THEN** Retrofit sends Authorization header with JWT from DataStore

### Requirement: Session cleared on unauthorized

The app SHALL clear stored token and navigate to login when API returns 401.

#### Scenario: Expired token

- **WHEN** `/api/auth/me` returns 401
- **THEN** app clears session and shows login flow
