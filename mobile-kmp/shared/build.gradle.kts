plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.kotlinSerialization)
    alias(libs.plugins.sqldelight)
}

val androidEnabled = providers.gradleProperty("android").orNull == "true"

kotlin {
    jvmToolchain(21)
    jvm()

    if (androidEnabled) {
        pluginManager.withPlugin("com.android.library") {
            androidTarget()
        }
    }

    sourceSets {
        commonMain.dependencies {
            implementation(libs.kotlinx.coroutines.core)
            implementation(libs.kotlinx.serialization.json)
            implementation(libs.ktor.client.core)
            implementation(libs.ktor.client.content.negotiation)
            implementation(libs.ktor.client.logging)
            implementation(libs.ktor.serialization.kotlinx.json)
            implementation(libs.sqldelight.runtime)
            implementation(libs.sqldelight.coroutines)
        }

        commonTest.dependencies {
            implementation(kotlin("test"))
        }

        jvmMain.dependencies {
            implementation(libs.sqldelight.sqlite.driver)
        }
    }
}

sqldelight {
    databases {
        create("UrbaPFDatabase") {
            packageName.set("com.urbapf.urbapf.db")
        }
    }
}