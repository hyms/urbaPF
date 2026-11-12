# Mobile KMP

Android-first Kotlin Multiplatform scaffold for UrbaPF.

- `shared/`: Kotlin Multiplatform module with SQLDelight persistence, Ktor client, kotlinx serialization, and outbox schema.
- `composeApp/`: Compose Multiplatform shared UI module (JVM/desktop target in this scaffold).
- `androidApp/`: Android entry point scaffold, enabled when an Android SDK is available.

## Validation

```bash
./gradlew :shared:build
./gradlew :composeApp:build
```

Android target is optional in this environment:

```bash
./gradlew -Pandroid=true :shared:build
```

The Android SDK and `ANDROID_HOME` are required for the Android target.