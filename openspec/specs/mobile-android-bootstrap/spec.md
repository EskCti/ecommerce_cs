## Requirements

### Requirement: Android project structure

The repository SHALL contain `apps/mobile-android` as a Kotlin project with Jetpack Compose enabled.

#### Scenario: Gradle assemble debug

- **WHEN** developer runs `./gradlew assembleDebug` in `apps/mobile-android`
- **THEN** the APK builds successfully

### Requirement: Dependency injection configured

The Android app SHALL use Hilt for dependency injection with a valid `@HiltAndroidApp` application class.

#### Scenario: Hilt graph compiles

- **WHEN** the project is compiled
- **THEN** Hilt generates components without missing binding errors

### Requirement: HTTP client points to API

Retrofit SHALL be configured with a base URL defaulting to `http://10.0.2.2:5000` for emulator access to the local API.

#### Scenario: Configurable base URL

- **WHEN** developer overrides base URL in local configuration
- **THEN** Retrofit uses the overridden URL for API calls
