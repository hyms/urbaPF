package com.urbapf.urbapf

import io.ktor.client.HttpClient
import io.ktor.client.plugins.contentencoding.ContentEncoding
import io.ktor.client.plugins.contentnegotiation.ContentNegotiation
import io.ktor.client.plugins.logging.LogLevel
import io.ktor.client.plugins.logging.Logger
import io.ktor.client.plugins.logging.Logging
import io.ktor.serialization.kotlinx.json.json
import kotlinx.serialization.json.Json

object NetworkClientFactory {
    fun create(): HttpClient = HttpClient {
        install(ContentNegotiation) {
            json(Json {
                ignoreUnknownKeys = true
                explicitNulls = false
            })
        }

        install(ContentEncoding) {
            gzip()
            deflate()
            br()
        }

        install(Logging) {
            logger = Logger.SIMPLE
            level = LogLevel.INFO
        }
    }
}