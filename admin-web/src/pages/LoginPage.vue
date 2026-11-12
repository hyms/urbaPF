<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { getHealth } from '@/api/health'

const { t } = useI18n()
const email = ref('')
const password = ref('')
const loading = ref(false)
const message = ref('')

async function checkApi() {
  loading.value = true
  try {
    const health = await getHealth()
    message.value = health.status === 'healthy' ? t('login.online') : t('login.offline')
  } catch {
    message.value = t('login.offline')
  } finally {
    loading.value = false
  }
}

function submit() {
  message.value = t('login.placeholder')
}
</script>

<template>
  <v-container class="fill-height" fluid>
    <v-row class="fill-height" align="center" justify="center">
      <v-col cols="12" sm="8" md="5">
        <v-card class="pa-2" elevation="3">
          <v-card-item>
            <v-card-title class="text-h5">{{ t('login.title') }}</v-card-title>
            <v-card-subtitle>{{ t('app.subtitle') }}</v-card-subtitle>
          </v-card-item>

          <v-card-text>
            <v-form @submit.prevent="submit">
              <v-text-field
                v-model="email"
                :label="t('login.email')"
                prepend-inner-icon="mdi-email-outline"
                type="email"
                autocomplete="username"
              />
              <v-text-field
                v-model="password"
                :label="t('login.password')"
                prepend-inner-icon="mdi-lock-outline"
                type="password"
                autocomplete="current-password"
              />
              <v-btn class="mt-2" color="primary" type="submit" block>{{ t('login.submit') }}</v-btn>
            </v-form>

            <v-alert v-if="message" class="mt-4" type="info" variant="tonal" density="comfortable">
              {{ message }}
            </v-alert>
          </v-card-text>

          <v-card-actions>
            <v-btn variant="text" :loading="loading" @click="checkApi">
              {{ t('login.checkApi') }}
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>