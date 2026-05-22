# RetailOps Mobile (Android)

Kotlin + Jetpack Compose + Hilt + Retrofit.

## Pré-requisitos

- Android SDK (API 35)
- JDK 17+

## Configuração

```bash
cp local.properties.example local.properties
# Edite sdk.dir e, se necessário, API_BASE_URL no app/build.gradle.kts
```

## Build

```bash
./gradlew assembleDebug
```

API padrão no emulador: `http://10.0.2.2:5000/` (via `BuildConfig.API_BASE_URL`).
