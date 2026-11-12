plugins {
    alias(libs.plugins.androidApplication)
    alias(libs.plugins.kotlinAndroid)
    alias(libs.plugins.composeCompiler)
}

android {
    namespace = "com.urbapf.urbapf.android"
    compileSdk = 35

    defaultConfig {
        applicationId = "com.urbapf.urbapf.android"
        minSdk = 26
        targetSdk = 35
        versionCode = 1
        versionName = "1.0.0"
    }

    buildFeatures {
        compose = true
    }
}

dependencies {
    implementation(project(":shared"))
    implementation(project(":composeApp"))
    implementation("androidx.activity:activity-compose:1.10.1")
}