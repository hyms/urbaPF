<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('alerts.title') }}</div>
    </div>

    <div class="row q-mb-md">
      <q-btn-toggle
        v-model="statusFilter"
        toggle-color="primary"
        :options="statusOptions"
        unelevated
      />
    </div>

    <div class="row q-col-gutter-md">
      <template v-if="loading">
        <div class="col-12" v-for="n in 3" :key="n">
          <q-card>
            <q-card-section>
              <q-skeleton type="text" class="q-mb-sm" style="width: 60%" />
              <q-skeleton type="text" style="width: 40%" />
            </q-card-section>
          </q-card>
        </div>
      </template>
<template v-else>
          <div v-for="alert in filteredAlerts" :key="alert.id" class="col-12 q-mb-md">
            <AlertItem
              :alert="alert"
              @approve="approveAlert"
              @acknowledge="acknowledgeAlert"
              @resolve="resolveAlert"
              @resend="resendAlert"
              @delete="deleteAlert"
            />
          </div>

          <div v-if="filteredAlerts.length === 0" class="col-12 text-center text-grey q-pa-xl">
            {{ t('alerts.noAlerts') }}
          </div>
        </template>
    </div>
  </q-page>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useAlertStore } from '../stores/alert'
import { useCondominiumStore } from '../stores/condominium'
import { useAuthStore } from '../stores/auth'
import { useI18n } from '../composables/useI18n'
import { formatDate } from '@/utils/format'
import { AlertTypeIcon, AlertStatusColor, AlertStatusLabel } from '@/utils/appEnums'
import AlertItem from '../components/alert/AlertItem.vue'

const $q = useQuasar()
const alertStore = useAlertStore()
const condoStore = useCondominiumStore()
const authStore = useAuthStore()
const { t } = useI18n()

const loading = ref(true)
const alerts = ref([])
const statusFilter = ref(null)

const statusOptions = computed(() => [
  { label: t('alerts.all'), value: null },
  { label: t('alerts.pending'), value: 1 },
  { label: t('alerts.active'), value: 3 }
])

const filteredAlerts = computed(() => {
  if (statusFilter.value === null) return alerts.value
  return alerts.value.filter(a => a.status === statusFilter.value)
})

onMounted(async () => {
  await fetchAlerts()
})

async function fetchAlerts() {
  loading.value = true
  try {
    const condos = await condoStore.fetchAll()
    if (condos.length > 0) {
      alerts.value = await alertStore.fetchByCondominium(condos[0].id)
    }
  } catch (e) {
    $q.notify({ type: 'negative', message: t('common.error') })
  } finally {
    loading.value = false
  }
}

async function approveAlert(alertId) {
  const result = await alertStore.approve(alertId)
  if (result) {
    $q.notify({ type: 'positive', message: t('common.success') })
    await fetchAlerts()
  }
}

async function acknowledgeAlert(alertId) {
  const result = await alertStore.acknowledge(alertId)
  if (result) {
    $q.notify({ type: 'positive', message: t('common.success') })
    await fetchAlerts()
  }
}

async function resolveAlert(alertId) {
  const result = await alertStore.resolve(alertId)
  if (result) {
    $q.notify({ type: 'positive', message: t('common.success') })
    await fetchAlerts()
  }
}

async function resendAlert(alertId) {
  const result = await alertStore.resend(alertId)
  if (result) {
    $q.notify({ type: 'positive', message: 'Alerta re-enviada' })
    await fetchAlerts()
  }
}

async function deleteAlert(alert) {
  $q.dialog({
    title: t('common.confirmDelete'),
    message: t('common.deleteMessage').replace('{item}', alert.title),
    cancel: true
  }).onOk(async () => {
    try {
      await alertStore.remove(alert.id)
      $q.notify({ type: 'positive', message: t('common.success') })
      await fetchAlerts()
    } catch (e) {
      $q.notify({ type: 'negative', message: t('common.error') })
    }
  })
}



</script>


