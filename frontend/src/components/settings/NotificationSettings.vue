<template>
  <q-card flat bordered>
    <q-card-section>
      <div class="text-h6 q-mb-md">{{ t('settings.notifPrefs') }}</div>
      <q-list>
        <q-item tag="label">
          <q-item-section>
            <q-item-label>{{ t('settings.notifIncidents') }}</q-item-label>
            <q-item-label caption>{{ t('settings.notifIncidentsDesc') }}</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-toggle v-model="form.incidents" />
          </q-item-section>
        </q-item>
        <q-item tag="label">
          <q-item-section>
            <q-item-label>{{ t('settings.notifAlerts') }}</q-item-label>
            <q-item-label caption>{{ t('settings.notifAlertsDesc') }}</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-toggle v-model="form.alerts" />
          </q-item-section>
        </q-item>
        <q-item tag="label">
          <q-item-section>
            <q-item-label>{{ t('settings.notifPolls') }}</q-item-label>
            <q-item-label caption>{{ t('settings.notifPollsDesc') }}</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-toggle v-model="form.polls" />
          </q-item-section>
        </q-item>
        <q-item tag="label">
          <q-item-section>
            <q-item-label>{{ t('settings.notifPosts') }}</q-item-label>
            <q-item-label caption>{{ t('settings.notifPostsDesc') }}</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-toggle v-model="form.posts" />
          </q-item-section>
        </q-item>
      </q-list>
    </q-card-section>
  </q-card>

  <q-card flat bordered class="q-mt-md">
    <q-card-section>
      <div class="text-h6 q-mb-md">{{ t('settings.fcmToken') }}</div>
      <q-input :model-value="fcmToken" :label="t('settings.fcmToken')" outlined readonly>
        <template v-slot:append>
          <q-btn flat round dense icon="content_copy" @click="copyToken" />
        </template>
      </q-input>
      <q-btn color="primary" :label="t('settings.updateToken')" @click="updateFcmToken" class="q-mt-md" :loading="loading" />
    </q-card-section>
  </q-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useQuasar } from 'quasar'

const props = defineProps<{
  notifications: { incidents: boolean; alerts: boolean; polls: boolean; posts: boolean }
  fcmToken: string
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:notifications', value: { incidents: boolean; alerts: boolean; polls: boolean; posts: boolean }): void
  (e: 'copyToken'): void
  (e: 'updateFcmToken'): void
}>()

const { t } = useI18n()
const $q = useQuasar()

const form = ref({ ...props.notifications })

let isInternalUpdate = false

watch(() => props.notifications, (newVal) => {
  if (!isInternalUpdate) {
    form.value = { ...newVal }
  }
}, { immediate: true })

watch(form, (newVal) => {
  isInternalUpdate = true
  emit('update:notifications', { ...newVal })
  setTimeout(() => { isInternalUpdate = false }, 0)
}, { deep: true })

const copyToken = () => {
  emit('copyToken')
}

const updateFcmToken = () => {
  emit('updateFcmToken')
}
</script>