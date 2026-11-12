<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { getHealth } from '@/api/health'
import { useAppStore } from '@/stores/app'

const { t } = useI18n()
const appStore = useAppStore()
const loading = ref(false)
const message = ref(t('health.idle'))
const status = ref<'idle' | 'ok' | 'error'>('idle')

async function check() {
  loading.value = true
  message.value = t('health.loading')
  try {
    const health = await getHealth()
    appStore.setApiOnline(health.status === 'healthy')
    status.value = health.status === 'healthy' ? 'ok' : 'error'
    message.value = health.status === 'healthy' ? t('health.ok') : t('health.error')
  } catch {
    appStore.setApiOnline(false)
    status.value = 'error'
    message.value = t('health.error')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <v-card class="mx-auto" max-width="720" elevation="2">
    <v-card-item>
      <v-card-title>{{ t('health.title') }}</v-card-title>
      <v-card-subtitle>{{ t('health.description') }}</v-card-subtitle>
    </v-card-item>

    <v-card-text>
      <v-chip :color="status === 'ok' ? 'success' : status === 'error' ? 'error' : 'medium-emphasis'" variant="tonal">
        {{ message }}
      </v-chip>
    </v-card-text>

    <v-card-actions>
      <v-btn color="primary" :loading="loading" @click="check">
        {{ t('health.check') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>