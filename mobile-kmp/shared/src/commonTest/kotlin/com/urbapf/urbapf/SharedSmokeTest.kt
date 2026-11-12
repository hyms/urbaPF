package com.urbapf.urbapf

import kotlin.test.Test
import kotlin.test.assertEquals

class SharedSmokeTest {
    @Test
    fun platformName_isConfigured() {
        assertEquals("jvm", platformName())
    }
}