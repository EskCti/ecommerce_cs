# RetailOps Mobile (Android)

Kotlin + Jetpack Compose + Hilt + Retrofit.

## Pré-requisitos

- Android SDK (API 35)
- JDK 17+

## Configuração

```bash
cp local.properties.example local.properties
# Edite sdk.dir se o SDK não estiver em $HOME/Android/Sdk
export ANDROID_HOME=$HOME/Android/Sdk   # ou caminho do Android Studio no Windows via /mnt/c/...
```

### SDK location not found

Crie `local.properties` com:

```properties
sdk.dir=/home/<user>/Android/Sdk
```

### Build Tools / Platform ausentes

O projeto usa **compileSdk 35** e **build-tools 34.0.0**. Instale com `sdkmanager`:

```bash
export ANDROID_HOME=$HOME/Android/Sdk

# Caminho típico no WSL (estrutura cmdline-tools aninhada):
SDKMANAGER="$ANDROID_HOME/cmdline-tools/cmdline-tools/bin/sdkmanager"

# Alternativa se Android Studio criou .../latest/bin/sdkmanager:
# SDKMANAGER="$ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager"

yes | "$SDKMANAGER" "build-tools;34.0.0" "platforms;android-35"
```

Se `latest/bin` estiver vazio, use o caminho `cmdline-tools/cmdline-tools/bin` acima ou reinstale as command-line tools pelo Android Studio (SDK Manager).

## Build

```bash
./gradlew assembleDebug
./gradlew testDebugUnitTest
```

API padrão no emulador: `http://10.0.2.2:5000/` (via `BuildConfig.API_BASE_URL`).
