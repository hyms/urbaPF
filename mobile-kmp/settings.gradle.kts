pluginManagement {
    repositories {
        google()
        maven("https://maven-central.storage-download.googleapis.com/maven2/")
        gradlePluginPortal()
    }
}

dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        maven("https://maven-central.storage-download.googleapis.com/maven2/")
    }
}

rootProject.name = "urbapf-mobile"

include(":shared")
include(":composeApp")
// androidApp is enabled when an Android SDK is available:
// ./gradlew -Pandroid=true ...
// include(":androidApp")